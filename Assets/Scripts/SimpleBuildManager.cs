using System.Collections.Generic;
using TowerDefense.Towers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class SimpleBuildManager : MonoBehaviour
{
    const float ButtonWidth = 240f;
    const float ButtonHeight = 92f;
    const int PreviewLayer = 30;

    public Camera placementCamera;
    public float placementHeight = 0.68f;
    public SimpleTowerArchetype[] buildOptions;
    public Vector3[] slotPositions;

    readonly List<SimpleBuildSlot> m_Slots = new List<SimpleBuildSlot>();
    readonly List<SimpleBuildButton> m_Buttons = new List<SimpleBuildButton>();

    SampleSceneBootstrap m_Bootstrap;
    GameObject m_CurrentGhost;
    SimpleTowerArchetype m_CurrentOption;
    SimpleBuildSlot m_CurrentHoveredSlot;
    bool m_CurrentPlacementValid;
    bool m_IsDragging;
    Plane m_PlacementPlane;
    Transform m_PreviewRoot;
    readonly List<RenderTexture> m_PreviewTextures = new List<RenderTexture>();

    public void Configure(
        SampleSceneBootstrap bootstrap,
        SimpleTowerArchetype[] options,
        Vector3[] positions,
        float worldHeight)
    {
        m_Bootstrap = bootstrap;
        buildOptions = options;
        slotPositions = positions;
        placementHeight = worldHeight;
        placementCamera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
        m_PlacementPlane = new Plane(Vector3.up, new Vector3(0f, placementHeight, 0f));

        EnsureEventSystem();
        EnsureSlots();
        EnsureUI();
        EnsurePreviews();
        UpdateAffordability();
        SubscribeCurrency();
    }

    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (!m_IsDragging || m_CurrentOption == null)
        {
            return;
        }

        UpdateDrag(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
        {
            EndDrag(Input.mousePosition);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearDragState();
        }
#endif
    }

    void OnDestroy()
    {
        UnsubscribeCurrency();
        ClearPreviewTextures();
        ClearDragState();
    }

    public void BeginDrag(SimpleTowerArchetype option, Vector2 screenPosition)
    {
        if (option == null || option.towerPrefab == null)
        {
            return;
        }

        ClearDragState();
        m_CurrentOption = option;
        m_IsDragging = true;
        m_CurrentGhost = Instantiate(option.towerPrefab.gameObject);
        m_CurrentGhost.name = option.displayName + "_PlacementGhost";
        DisableGhostBehaviour(m_CurrentGhost);
        UpdateDrag(screenPosition);
    }

    public void UpdateDrag(Vector2 screenPosition)
    {
        if (m_CurrentGhost == null || m_CurrentOption == null)
        {
            return;
        }

        Vector3 worldPosition = GetWorldPoint(screenPosition);
        SimpleBuildSlot hoveredSlot = GetHoveredSlot(screenPosition);
        bool canAfford = SimpleCurrencyManager.instance == null ||
                         SimpleCurrencyManager.instance.CanAfford(m_CurrentOption.cost);

        ClearSlotPreview();
        m_CurrentHoveredSlot = hoveredSlot;
        m_CurrentPlacementValid = hoveredSlot != null && hoveredSlot.canPlace && canAfford;

        if (hoveredSlot != null)
        {
            hoveredSlot.SetPreview(true, m_CurrentPlacementValid);
            worldPosition = hoveredSlot.transform.position;
        }

        worldPosition.y = placementHeight;
        m_CurrentGhost.transform.position = worldPosition;
        ApplyGhostColor(m_CurrentGhost, m_CurrentPlacementValid ? m_CurrentOption.uiColor : new Color(0.85f, 0.12f, 0.12f, 0.75f));
    }

    public void EndDrag(Vector2 screenPosition)
    {
        if (m_CurrentGhost == null || m_CurrentOption == null)
        {
            return;
        }

        UpdateDrag(screenPosition);

        bool placed = false;
        if (m_CurrentPlacementValid &&
            m_CurrentHoveredSlot != null &&
            m_CurrentHoveredSlot.canPlace &&
            SimpleCurrencyManager.instance != null &&
            SimpleCurrencyManager.instance.TrySpendCurrency(m_CurrentOption.cost))
        {
            Tower tower = m_Bootstrap.PlaceBuiltTower(m_CurrentOption, m_CurrentHoveredSlot.transform.position);
            if (tower != null)
            {
                m_CurrentHoveredSlot.SetOccupied(true);
                placed = true;
            }
        }

        ClearDragState();

        if (!placed)
        {
            UpdateAffordability();
        }
    }

    void EnsureSlots()
    {
        if (slotPositions == null || slotPositions.Length == 0 || m_Slots.Count > 0)
        {
            return;
        }

        for (int i = 0; i < slotPositions.Length; i++)
        {
            var slotObject = new GameObject("BuildSlot_" + (i + 1));
            slotObject.transform.SetParent(transform, false);
            slotObject.transform.position = slotPositions[i];
            var slot = slotObject.AddComponent<SimpleBuildSlot>();
            m_Slots.Add(slot);
        }
    }

    void EnsureUI()
    {
        if (m_Buttons.Count > 0 || buildOptions == null || buildOptions.Length == 0)
        {
            return;
        }

        Font font = LoadBuiltInFont();

        var canvasObject = new GameObject("SimpleBuildCanvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        var barObject = new GameObject("BottomBar");
        barObject.transform.SetParent(canvasObject.transform, false);

        var barRect = barObject.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0f);
        barRect.anchorMax = new Vector2(0.5f, 0f);
        barRect.pivot = new Vector2(0.5f, 0f);
        barRect.anchoredPosition = new Vector2(0f, 18f);
        barRect.sizeDelta = new Vector2((ButtonWidth + 18f) * buildOptions.Length + 32f, 128f);

        var barImage = barObject.AddComponent<Image>();
        barImage.color = new Color(0.07f, 0.09f, 0.12f, 0.92f);

        for (int i = 0; i < buildOptions.Length; i++)
        {
            var buttonObject = new GameObject(buildOptions[i].displayName + "_Button");
            buttonObject.transform.SetParent(barObject.transform, false);

            var buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0f, 0.5f);
            buttonRect.anchorMax = new Vector2(0f, 0.5f);
            buttonRect.pivot = new Vector2(0f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(16f + i * (ButtonWidth + 18f), 0f);
            buttonRect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

            var image = buttonObject.AddComponent<Image>();
            image.raycastTarget = true;

            var buildButton = buttonObject.AddComponent<SimpleBuildButton>();
            buildButton.Configure(this, buildOptions[i], font);
            m_Buttons.Add(buildButton);
        }
    }

    void EnsurePreviews()
    {
        if (m_PreviewRoot != null || buildOptions == null || buildOptions.Length == 0 || m_Buttons.Count != buildOptions.Length)
        {
            return;
        }

        var previewRootObject = new GameObject("SimpleBuildPreviews");
        previewRootObject.transform.SetParent(transform, false);
        previewRootObject.transform.position = new Vector3(0f, -5000f, 0f);
        m_PreviewRoot = previewRootObject.transform;

        for (int i = 0; i < buildOptions.Length; i++)
        {
            RenderTexture previewTexture = CreatePreview(buildOptions[i], i);
            if (previewTexture != null)
            {
                m_Buttons[i].SetPreviewTexture(previewTexture);
                m_PreviewTextures.Add(previewTexture);
            }
        }
    }

    void EnsureEventSystem()
    {
        var eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (legacyModule != null)
        {
            legacyModule.enabled = false;
            Destroy(legacyModule);
        }
#else
        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }

    void SubscribeCurrency()
    {
        if (SimpleCurrencyManager.instance != null)
        {
            SimpleCurrencyManager.instance.currencyChanged -= OnCurrencyChanged;
            SimpleCurrencyManager.instance.currencyChanged += OnCurrencyChanged;
        }
    }

    void UnsubscribeCurrency()
    {
        if (SimpleCurrencyManager.instance != null)
        {
            SimpleCurrencyManager.instance.currencyChanged -= OnCurrencyChanged;
        }
    }

    RenderTexture CreatePreview(SimpleTowerArchetype option, int index)
    {
        if (option == null || option.towerPrefab == null)
        {
            return null;
        }

        var previewAnchor = new GameObject(option.displayName + "_PreviewAnchor").transform;
        previewAnchor.SetParent(m_PreviewRoot, false);
        previewAnchor.localPosition = new Vector3(index * 8f, 0f, 0f);

        var previewTower = Instantiate(option.towerPrefab.gameObject, previewAnchor);
        previewTower.name = option.displayName + "_PreviewModel";
        previewTower.transform.localPosition = Vector3.zero;
        previewTower.transform.localRotation = Quaternion.Euler(0f, 145f, 0f);
        DisableGhostBehaviour(previewTower);
        SetLayerRecursively(previewTower, PreviewLayer);

        Bounds previewBounds = CalculateBounds(previewTower);

        var cameraObject = new GameObject(option.displayName + "_PreviewCamera");
        cameraObject.transform.SetParent(previewAnchor, false);
        cameraObject.transform.position = previewBounds.center + new Vector3(1.7f, previewBounds.extents.y * 1.5f + 0.4f, -2.2f);
        cameraObject.transform.LookAt(previewBounds.center + Vector3.up * 0.2f);

        var previewCamera = cameraObject.AddComponent<Camera>();
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        previewCamera.cullingMask = 1 << PreviewLayer;
        previewCamera.fieldOfView = 24f;
        previewCamera.nearClipPlane = 0.01f;
        previewCamera.farClipPlane = 20f;
        previewCamera.allowHDR = false;
        previewCamera.allowMSAA = false;

        var previewTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        previewTexture.name = option.displayName + "_PreviewTexture";
        previewTexture.Create();
        previewCamera.targetTexture = previewTexture;
        previewCamera.Render();
        return previewTexture;
    }

    void OnCurrencyChanged(int currentValue)
    {
        UpdateAffordability();
    }

    void UpdateAffordability()
    {
        bool hasCurrencyManager = SimpleCurrencyManager.instance != null;
        foreach (var button in m_Buttons)
        {
            bool canAfford = !hasCurrencyManager || button.buildOption == null ||
                             SimpleCurrencyManager.instance.CanAfford(button.buildOption.cost);
            button.SetAffordable(canAfford);
        }
    }

    void DisableGhostBehaviour(GameObject ghostObject)
    {
        if (ghostObject == null)
        {
            return;
        }

        foreach (var behaviour in ghostObject.GetComponentsInChildren<Behaviour>(true))
        {
            behaviour.enabled = false;
        }

        foreach (var collider in ghostObject.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }
    }

    void SetLayerRecursively(GameObject rootObject, int layer)
    {
        if (rootObject == null)
        {
            return;
        }

        rootObject.layer = layer;
        foreach (Transform child in rootObject.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    Bounds CalculateBounds(GameObject rootObject)
    {
        var renderers = rootObject.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(rootObject.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    void ApplyGhostColor(GameObject ghostObject, Color targetColor)
    {
        if (ghostObject == null)
        {
            return;
        }

        var renderers = ghostObject.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty("_Color"))
                {
                    material.color = targetColor;
                }

                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", targetColor);
                }
            }
        }
    }

    Vector3 GetWorldPoint(Vector2 screenPosition)
    {
        if (placementCamera == null)
        {
            return new Vector3(0f, placementHeight, 0f);
        }

        Ray ray = placementCamera.ScreenPointToRay(screenPosition);
        float enter;
        if (m_PlacementPlane.Raycast(ray, out enter))
        {
            return ray.GetPoint(enter);
        }

        return new Vector3(0f, placementHeight, 0f);
    }

    SimpleBuildSlot GetHoveredSlot(Vector2 screenPosition)
    {
        if (placementCamera == null)
        {
            return null;
        }

        Ray ray = placementCamera.ScreenPointToRay(screenPosition);
        var hits = Physics.RaycastAll(ray, 500f, ~0, QueryTriggerInteraction.Collide);
        foreach (var hit in hits)
        {
            var slot = hit.collider.GetComponentInParent<SimpleBuildSlot>();
            if (slot != null)
            {
                return slot;
            }
        }

        return null;
    }

    void ClearSlotPreview()
    {
        foreach (var slot in m_Slots)
        {
            if (slot != null)
            {
                slot.SetPreview(false, false);
            }
        }
    }

    void ClearDragState()
    {
        ClearSlotPreview();

        if (m_CurrentGhost != null)
        {
            Destroy(m_CurrentGhost);
        }

        m_IsDragging = false;
        m_CurrentGhost = null;
        m_CurrentOption = null;
        m_CurrentHoveredSlot = null;
        m_CurrentPlacementValid = false;
    }

    void ClearPreviewTextures()
    {
        foreach (var previewTexture in m_PreviewTextures)
        {
            if (previewTexture != null)
            {
                previewTexture.Release();
                Destroy(previewTexture);
            }
        }

        m_PreviewTextures.Clear();
    }

    Font LoadBuiltInFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }
}

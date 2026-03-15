using UnityEngine;
using UnityEngine.UI;

public class SimpleCurrencyHUD : MonoBehaviour
{
    public SimpleCurrencyManager currencyManager;
    public string label = "Moedas";
    public string currencySymbol = "";
    [SerializeField] Text display;

    void Awake()
    {
        if (currencyManager == null)
        {
            currencyManager = SimpleCurrencyManager.instance;
        }

        EnsureUI();
    }

    void OnEnable()
    {
        if (currencyManager == null)
        {
            currencyManager = SimpleCurrencyManager.instance;
        }

        if (currencyManager != null)
        {
            currencyManager.currencyChanged += OnCurrencyChanged;
        }

        UpdateDisplay();
    }

    void OnDisable()
    {
        if (currencyManager != null)
        {
            currencyManager.currencyChanged -= OnCurrencyChanged;
        }
    }

    public void Assign(SimpleCurrencyManager manager)
    {
        if (currencyManager != null)
        {
            currencyManager.currencyChanged -= OnCurrencyChanged;
        }

        currencyManager = manager;

        if (isActiveAndEnabled && currencyManager != null)
        {
            currencyManager.currencyChanged += OnCurrencyChanged;
        }

        UpdateDisplay();
    }

    void EnsureUI()
    {
        if (display != null)
        {
            return;
        }

        var canvasObject = new GameObject("SimpleCurrencyCanvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        var panelObject = new GameObject("CurrencyPanel");
        panelObject.transform.SetParent(canvasObject.transform, false);

        var panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(24f, -24f);
        panelRect.sizeDelta = new Vector2(260f, 60f);

        var panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.11f, 0.14f, 0.88f);

        var textObject = new GameObject("CurrencyText");
        textObject.transform.SetParent(panelObject.transform, false);

        var textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 10f);
        textRect.offsetMax = new Vector2(-16f, -10f);

        display = textObject.AddComponent<Text>();
        display.alignment = TextAnchor.MiddleLeft;
        display.fontSize = 26;
        display.color = new Color(0.96f, 0.91f, 0.67f, 1f);
        display.horizontalOverflow = HorizontalWrapMode.Overflow;
        display.verticalOverflow = VerticalWrapMode.Overflow;
        display.font = LoadBuiltInFont();
    }

    void OnCurrencyChanged(int currentCurrency)
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (display == null)
        {
            return;
        }

        int currentCurrency = currencyManager == null ? 0 : currencyManager.currentCurrency;
        display.text = label + ": " + currencySymbol + currentCurrency;
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

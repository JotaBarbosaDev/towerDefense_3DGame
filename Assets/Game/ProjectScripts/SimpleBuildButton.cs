using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleBuildButton : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SimpleBuildManager buildManager;
    public SimpleTowerArchetype buildOption;

    [SerializeField] Image background;
    [SerializeField] Image accentBar;
    [SerializeField] RawImage previewImage;
    [SerializeField] Text titleLabel;
    [SerializeField] Text costLabel;

    public void Configure(SimpleBuildManager manager, SimpleTowerArchetype option, Font font)
    {
        buildManager = manager;
        buildOption = option;

        if (background == null)
        {
            background = GetComponent<Image>();
        }

        if (accentBar == null)
        {
            accentBar = CreateImage("Accent", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -6f), new Vector2(8f, -6f));
        }

        if (previewImage == null)
        {
            previewImage = CreateRawImage("Preview", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(72f, 72f));
        }

        if (titleLabel == null)
        {
            titleLabel = CreateText("Title", font, 20, TextAnchor.UpperLeft, new Vector2(104f, -10f), new Vector2(-14f, -42f));
        }

        if (costLabel == null)
        {
            costLabel = CreateText("Cost", font, 18, TextAnchor.LowerLeft, new Vector2(104f, 18f), new Vector2(-14f, -12f));
        }

        if (background != null)
        {
            background.color = new Color(0.11f, 0.13f, 0.17f, 0.96f);
        }

        if (accentBar != null)
        {
            accentBar.color = option.uiColor;
            accentBar.raycastTarget = false;
        }

        titleLabel.text = option.displayName;
        costLabel.text = option.cost + " moedas";
    }

    public void SetAffordable(bool canAfford)
    {
        if (background == null || buildOption == null)
        {
            return;
        }

        if (background != null)
        {
            background.color = canAfford
                ? new Color(0.11f, 0.13f, 0.17f, 0.96f)
                : new Color(0.11f, 0.13f, 0.17f, 0.56f);
        }

        if (accentBar != null)
        {
            Color accentColor = buildOption.uiColor;
            accentColor.a = canAfford ? 1f : 0.35f;
            accentBar.color = accentColor;
        }

        if (titleLabel != null)
        {
            titleLabel.color = canAfford
                ? new Color(0.94f, 0.96f, 0.98f, 1f)
                : new Color(0.7f, 0.73f, 0.78f, 1f);
        }

        if (costLabel != null)
        {
            costLabel.color = canAfford
                ? new Color(0.95f, 0.84f, 0.47f, 1f)
                : new Color(0.58f, 0.58f, 0.58f, 1f);
        }
    }

    public void SetPreviewTexture(RenderTexture texture)
    {
        if (previewImage != null)
        {
            previewImage.texture = texture;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (buildManager != null && buildOption != null)
        {
            buildManager.BeginDrag(buildOption, eventData.position);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (buildManager != null && buildOption != null)
        {
            buildManager.BeginDrag(buildOption, eventData.position);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (buildManager != null)
        {
            buildManager.UpdateDrag(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (buildManager != null)
        {
            buildManager.EndDrag(eventData.position);
        }
    }

    Image CreateImage(
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        var imageObject = new GameObject(objectName);
        imageObject.transform.SetParent(transform, false);

        var imageRect = imageObject.AddComponent<RectTransform>();
        imageRect.anchorMin = anchorMin;
        imageRect.anchorMax = anchorMax;
        imageRect.pivot = pivot;
        imageRect.offsetMin = offsetMin;
        imageRect.offsetMax = offsetMax;

        return imageObject.AddComponent<Image>();
    }

    RawImage CreateRawImage(
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        var imageObject = new GameObject(objectName);
        imageObject.transform.SetParent(transform, false);

        var imageRect = imageObject.AddComponent<RectTransform>();
        imageRect.anchorMin = anchorMin;
        imageRect.anchorMax = anchorMax;
        imageRect.pivot = pivot;
        imageRect.anchoredPosition = anchoredPosition;
        imageRect.sizeDelta = sizeDelta;

        var image = imageObject.AddComponent<RawImage>();
        image.raycastTarget = false;
        return image;
    }

    Text CreateText(string objectName, Font font, int fontSize, TextAnchor alignment, Vector2 offsetMin, Vector2 offsetMax)
    {
        var textObject = new GameObject(objectName);
        textObject.transform.SetParent(transform, false);

        var textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = offsetMin;
        textRect.offsetMax = offsetMax;

        var text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = new Color(0.94f, 0.96f, 0.98f, 1f);
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }
}

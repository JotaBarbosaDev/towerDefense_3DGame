using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SimpleBuildSlot : MonoBehaviour
{
    public Color availableColor = new Color(0.16f, 0.67f, 0.45f, 1f);
    public Color previewValidColor = new Color(0.95f, 0.76f, 0.21f, 1f);
    public Color previewInvalidColor = new Color(0.85f, 0.19f, 0.19f, 1f);
    public Color occupiedColor = new Color(0.23f, 0.26f, 0.31f, 1f);

    [SerializeField] Renderer slotRenderer;
    [SerializeField] SphereCollider slotCollider;
    bool m_IsOccupied;

    public bool isOccupied
    {
        get { return m_IsOccupied; }
    }

    public bool canPlace
    {
        get { return !m_IsOccupied; }
    }

    void Awake()
    {
        EnsureSlotVisual();
        SetOccupied(false);
    }

    public void SetOccupied(bool occupied)
    {
        m_IsOccupied = occupied;
        ApplyColor(occupied ? occupiedColor : availableColor);
    }

    public void SetPreview(bool active, bool valid)
    {
        if (m_IsOccupied)
        {
            ApplyColor(occupiedColor);
            return;
        }

        if (!active)
        {
            ApplyColor(availableColor);
            return;
        }

        ApplyColor(valid ? previewValidColor : previewInvalidColor);
    }

    void EnsureSlotVisual()
    {
        if (slotRenderer == null)
        {
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "Visual";
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(1.6f, 0.03f, 1.6f);

            var visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Destroy(visualCollider);
            }

            slotRenderer = visual.GetComponent<Renderer>();
        }

        if (slotCollider == null)
        {
            slotCollider = GetComponent<SphereCollider>();
        }

        slotCollider.isTrigger = true;
        slotCollider.radius = 1.1f;
        slotCollider.center = Vector3.zero;
    }

    void ApplyColor(Color color)
    {
        if (slotRenderer == null)
        {
            return;
        }

        var materials = slotRenderer.materials;
        foreach (var material in materials)
        {
            if (material == null)
            {
                continue;
            }

            if (material.HasProperty("_Color"))
            {
                material.color = color;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
        }
    }
}

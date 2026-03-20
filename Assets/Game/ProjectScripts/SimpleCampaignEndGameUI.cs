using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimpleCampaignEndGameUI : MonoBehaviour
{
    SimpleHomeBaseHealth m_HomeBase;
    string m_NextSceneName;
    Font m_Font;
    Canvas m_Canvas;
    Text m_MessageLabel;
    Text m_StarsLabel;
    GameObject m_NextButton;
    bool m_SpawningCompleted;
    bool m_LevelFinished;

    public void Configure(SimpleHomeBaseHealth homeBase, string nextSceneName)
    {
        m_HomeBase = homeBase;
        m_NextSceneName = nextSceneName;
        InputSystemEventSystemBootstrap.EnsureInputSystemModules();
        EnsureUI();
    }

    public void NotifySpawningCompleted()
    {
        m_SpawningCompleted = true;
    }

    void Update()
    {
        if (m_LevelFinished || m_HomeBase == null)
        {
            return;
        }

        if (m_HomeBase.isDestroyed)
        {
            FinishLevel(false);
            return;
        }

        if (m_SpawningCompleted &&
            Object.FindObjectsByType<SimpleEnemyTargetable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length == 0)
        {
            FinishLevel(true);
        }
    }

    void FinishLevel(bool isVictory)
    {
        if (m_LevelFinished)
        {
            return;
        }

        m_LevelFinished = true;
        EnsureUI();
        m_Canvas.enabled = true;

        if (isVictory)
        {
            int stars = CalculateStars();
            CampaignProgression.SetStarsForScene(SceneManager.GetActiveScene().name, stars);
            m_MessageLabel.text = "Nivel Completo";
            m_StarsLabel.text = BuildStarsText(stars);
            m_NextButton.SetActive(!string.IsNullOrEmpty(m_NextSceneName));
        }
        else
        {
            m_MessageLabel.text = "Nivel Falhado";
            m_StarsLabel.text = "Sem estrelas";
            m_NextButton.SetActive(false);
        }

        Time.timeScale = 0f;
    }

    int CalculateStars()
    {
        if (m_HomeBase == null || m_HomeBase.maxHealth <= 0)
        {
            return 1;
        }

        float ratio = (float)m_HomeBase.currentHealth / m_HomeBase.maxHealth;
        if (Mathf.Approximately(ratio, 1f))
        {
            return 3;
        }

        if (ratio >= 0.5f)
        {
            return 2;
        }

        if (ratio > 0f)
        {
            return 1;
        }

        return 0;
    }

    string BuildStarsText(int stars)
    {
        switch (stars)
        {
            case 3:
                return "Estrelas: ***";
            case 2:
                return "Estrelas: **";
            case 1:
                return "Estrelas: *";
            default:
                return "Sem estrelas";
        }
    }

    void RestartLevel()
    {
        Time.timeScale = 1f;
        string currentScene = SceneManager.GetActiveScene().name;
        Level1GameSession.RequestLevelStart(Level1GameSession.selectedDifficulty, currentScene);
        SceneManager.LoadScene(currentScene);
    }

    void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(Level1GameSession.MenuSceneName);
    }

    void GoToNextLevel()
    {
        if (string.IsNullOrEmpty(m_NextSceneName))
        {
            return;
        }

        Time.timeScale = 1f;
        Level1GameSession.RequestLevelStart(Level1GameSession.selectedDifficulty, m_NextSceneName);
        SceneManager.LoadScene(m_NextSceneName);
    }

    void EnsureUI()
    {
        if (m_Canvas != null)
        {
            return;
        }

        m_Font = LoadBuiltInFont();

        var canvasObject = new GameObject("SimpleCampaignEndGameCanvas");
        canvasObject.transform.SetParent(transform, false);
        m_Canvas = canvasObject.AddComponent<Canvas>();
        m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        m_Canvas.sortingOrder = 6000;
        m_Canvas.enabled = false;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        CreateImage(
            "Backdrop",
            canvasObject.transform,
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Color(0.03f, 0.05f, 0.08f, 0.78f));

        var panel = CreatePanel(canvasObject.transform, new Vector2(0.5f, 0.5f), new Vector2(520f, 320f));
        m_MessageLabel = CreateText(panel.transform, "Message", string.Empty, 40, new Vector2(36f, -38f), new Vector2(320f, 48f));
        m_StarsLabel = CreateText(panel.transform, "Stars", string.Empty, 24, new Vector2(38f, -102f), new Vector2(220f, 36f));

        CreateButton(panel.transform, "Menu", new Vector2(36f, -232f), new Vector2(130f, 52f), GoToMenu);
        CreateButton(panel.transform, "Reiniciar", new Vector2(190f, -232f), new Vector2(130f, 52f), RestartLevel);
        m_NextButton = CreateButton(panel.transform, "Proximo", new Vector2(344f, -232f), new Vector2(140f, 52f), GoToNextLevel).gameObject;
    }

    GameObject CreatePanel(Transform parent, Vector2 anchor, Vector2 size)
    {
        var panelObject = new GameObject("Panel");
        panelObject.transform.SetParent(parent, false);

        var rectTransform = panelObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;

        var image = panelObject.AddComponent<Image>();
        image.color = new Color(0.1f, 0.13f, 0.17f, 0.96f);
        return panelObject;
    }

    Image CreateImage(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var imageObject = new GameObject(objectName);
        imageObject.transform.SetParent(parent, false);

        var rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        var image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    Text CreateText(Transform parent, string objectName, string value, int fontSize, Vector2 anchoredPosition, Vector2 size)
    {
        var textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        var rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        var text = textObject.AddComponent<Text>();
        text.font = m_Font;
        text.text = value;
        text.fontSize = fontSize;
        text.color = new Color(0.96f, 0.97f, 0.99f, 1f);
        text.alignment = TextAnchor.UpperLeft;
        return text;
    }

    Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = new GameObject(label + "_Button");
        buttonObject.transform.SetParent(parent, false);

        var rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.23f, 0.29f, 1f);

        var button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        var labelText = CreateText(buttonObject.transform, "Label", label, 22, new Vector2(0f, 0f), size);
        labelText.alignment = TextAnchor.MiddleCenter;

        return button;
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

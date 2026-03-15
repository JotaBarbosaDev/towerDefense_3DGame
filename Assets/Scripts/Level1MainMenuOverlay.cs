using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Level1MainMenuOverlay : MonoBehaviour
{
    const string VolumeKey = "Level1MasterVolume";

    Canvas m_Canvas;
    Font m_Font;
    GameObject m_MainPanel;
    GameObject m_DifficultyPanel;
    GameObject m_LevelPanel;
    GameObject m_SettingsPanel;
    Text m_CurrentDifficultyLabel;
    Text m_LevelSelectionLabel;
    Text m_VolumeValueLabel;
    Level1Difficulty m_SelectedDifficulty = Level1Difficulty.Medium;

    void Awake()
    {
        if (SceneManager.GetActiveScene().name != Level1GameSession.MenuSceneName)
        {
            Destroy(gameObject);
            return;
        }

        EnsureEventSystem();
        LoadVolume();
        m_SelectedDifficulty = Level1GameSession.selectedDifficulty;
        BuildUI();
        ShowMainPanel();
    }

    void BuildUI()
    {
        m_Font = LoadBuiltInFont();

        var canvasObject = new GameObject("Level1MenuCanvas");
        canvasObject.transform.SetParent(transform, false);

        m_Canvas = canvasObject.AddComponent<Canvas>();
        m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        m_Canvas.sortingOrder = 5000;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        var backdrop = CreateImage(
            "Backdrop",
            canvasObject.transform,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            new Color(0.03f, 0.05f, 0.07f, 0.76f));
        backdrop.raycastTarget = true;

        CreateImage(
            "TopStripe",
            canvasObject.transform,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -26f),
            new Vector2(0f, 0f),
            new Color(0.98f, 0.75f, 0.35f, 0.95f));

        var title = CreateText(
            "Title",
            canvasObject.transform,
            "Tower Defense - Campanha",
            48,
            TextAnchor.UpperLeft,
            new Vector2(64f, -52f),
            new Vector2(640f, 72f),
            new Color(0.97f, 0.98f, 0.99f, 1f));
        title.fontStyle = FontStyle.Bold;

        CreateText(
            "Subtitle",
            canvasObject.transform,
            "Escolhe a dificuldade, seleciona o nivel e entra com a estrategia certa.",
            22,
            TextAnchor.UpperLeft,
            new Vector2(66f, -110f),
            new Vector2(900f, 48f),
            new Color(0.78f, 0.84f, 0.9f, 1f));

        m_CurrentDifficultyLabel = CreateText(
            "CurrentDifficulty",
            canvasObject.transform,
            string.Empty,
            20,
            TextAnchor.UpperLeft,
            new Vector2(66f, -156f),
            new Vector2(620f, 40f),
            new Color(0.95f, 0.84f, 0.47f, 1f));

        m_MainPanel = CreatePanel("MainPanel", canvasObject.transform, new Vector2(90f, -230f), new Vector2(540f, 420f));
        m_DifficultyPanel = CreatePanel("DifficultyPanel", canvasObject.transform, new Vector2(90f, -230f), new Vector2(920f, 620f));
        m_LevelPanel = CreatePanel("LevelPanel", canvasObject.transform, new Vector2(90f, -230f), new Vector2(920f, 540f));
        m_SettingsPanel = CreatePanel("SettingsPanel", canvasObject.transform, new Vector2(90f, -230f), new Vector2(700f, 360f));

        BuildMainPanel();
        BuildDifficultyPanel();
        BuildLevelPanel();
        BuildSettingsPanel();
        RefreshDifficultyLabel();
    }

    void BuildMainPanel()
    {
        CreateText(
            "MenuTitle",
            m_MainPanel.transform,
            "Menu Inicial",
            32,
            TextAnchor.UpperLeft,
            new Vector2(32f, -28f),
            new Vector2(260f, 44f),
            new Color(0.96f, 0.97f, 0.99f, 1f));

        CreateMenuButton(m_MainPanel.transform, "Comecar Jogo", new Vector2(32f, -112f), new Vector2(460f, 72f), ShowDifficultyPanel);
        CreateMenuButton(m_MainPanel.transform, "Definicoes", new Vector2(32f, -206f), new Vector2(460f, 72f), ShowSettingsPanel);
        CreateMenuButton(m_MainPanel.transform, "Sair", new Vector2(32f, -300f), new Vector2(460f, 72f), QuitGame);
    }

    void BuildDifficultyPanel()
    {
        CreateText(
            "DifficultyTitle",
            m_DifficultyPanel.transform,
            "Escolher Dificuldade",
            32,
            TextAnchor.UpperLeft,
            new Vector2(32f, -28f),
            new Vector2(420f, 44f),
            new Color(0.96f, 0.97f, 0.99f, 1f));

        CreateText(
            "DifficultyHint",
            m_DifficultyPanel.transform,
            "Cada dificuldade altera vida dos inimigos, economia inicial e arsenal disponivel.",
            20,
            TextAnchor.UpperLeft,
            new Vector2(34f, -74f),
            new Vector2(780f, 42f),
            new Color(0.76f, 0.82f, 0.88f, 1f));

        Level1Difficulty[] difficulties =
        {
            Level1Difficulty.Easy,
            Level1Difficulty.Medium,
            Level1Difficulty.Hard,
            Level1Difficulty.Epic
        };

        for (int i = 0; i < difficulties.Length; i++)
        {
            Level1DifficultyConfig config = Level1GameSession.GetConfig(difficulties[i]);
            CreateDifficultyButton(
                m_DifficultyPanel.transform,
                config,
                new Vector2(32f, -136f - (i * 108f)),
                new Vector2(860f, 84f));
        }

        CreateMenuButton(m_DifficultyPanel.transform, "Voltar", new Vector2(32f, -570f), new Vector2(260f, 64f), ShowMainPanel);
    }

    void BuildLevelPanel()
    {
        CreateText(
            "LevelTitle",
            m_LevelPanel.transform,
            "Escolher Nivel",
            32,
            TextAnchor.UpperLeft,
            new Vector2(32f, -28f),
            new Vector2(320f, 44f),
            new Color(0.96f, 0.97f, 0.99f, 1f));

        m_LevelSelectionLabel = CreateText(
            "LevelDifficulty",
            m_LevelPanel.transform,
            string.Empty,
            20,
            TextAnchor.UpperLeft,
            new Vector2(34f, -74f),
            new Vector2(740f, 36f),
            new Color(0.95f, 0.84f, 0.47f, 1f));

        for (int levelIndex = 1; levelIndex <= 5; levelIndex++)
        {
            string sceneName = Level1GameSession.GetSceneNameForLevel(levelIndex);
            string buttonLabel = levelIndex == 1
                ? "Nivel 1 - Teu nivel jogavel"
                : "Nivel " + levelIndex;

            CreateLevelButton(
                m_LevelPanel.transform,
                buttonLabel,
                sceneName,
                new Vector2(32f, -128f - ((levelIndex - 1) * 72f)),
                new Vector2(520f, 56f));
        }

        CreateMenuButton(m_LevelPanel.transform, "Voltar", new Vector2(32f, -474f), new Vector2(260f, 56f), ShowDifficultyPanel);
    }

    void BuildSettingsPanel()
    {
        CreateText(
            "SettingsTitle",
            m_SettingsPanel.transform,
            "Definicoes",
            32,
            TextAnchor.UpperLeft,
            new Vector2(32f, -28f),
            new Vector2(280f, 44f),
            new Color(0.96f, 0.97f, 0.99f, 1f));

        CreateText(
            "VolumeLabel",
            m_SettingsPanel.transform,
            "Volume Geral",
            22,
            TextAnchor.UpperLeft,
            new Vector2(34f, -102f),
            new Vector2(220f, 32f),
            new Color(0.83f, 0.88f, 0.93f, 1f));

        Slider volumeSlider = CreateSlider(m_SettingsPanel.transform, new Vector2(34f, -152f), new Vector2(520f, 26f));
        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        m_VolumeValueLabel = CreateText(
            "VolumeValue",
            m_SettingsPanel.transform,
            Mathf.RoundToInt(AudioListener.volume * 100f) + "%",
            22,
            TextAnchor.MiddleRight,
            new Vector2(566f, -160f),
            new Vector2(88f, 36f),
            new Color(0.95f, 0.84f, 0.47f, 1f));

        CreateText(
            "SettingsHint",
            m_SettingsPanel.transform,
            "As definicoes deste menu ficam guardadas para a proxima vez que abrires o nivel 1.",
            18,
            TextAnchor.UpperLeft,
            new Vector2(34f, -218f),
            new Vector2(610f, 48f),
            new Color(0.7f, 0.77f, 0.84f, 1f));

        CreateMenuButton(m_SettingsPanel.transform, "Voltar", new Vector2(34f, -284f), new Vector2(220f, 56f), ShowMainPanel);
    }

    void ShowMainPanel()
    {
        SetPanelState(m_MainPanel, true);
        SetPanelState(m_DifficultyPanel, false);
        SetPanelState(m_LevelPanel, false);
        SetPanelState(m_SettingsPanel, false);
        RefreshDifficultyLabel();
    }

    void ShowDifficultyPanel()
    {
        SetPanelState(m_MainPanel, false);
        SetPanelState(m_DifficultyPanel, true);
        SetPanelState(m_LevelPanel, false);
        SetPanelState(m_SettingsPanel, false);
    }

    void ShowLevelPanel()
    {
        SetPanelState(m_MainPanel, false);
        SetPanelState(m_DifficultyPanel, false);
        SetPanelState(m_LevelPanel, true);
        SetPanelState(m_SettingsPanel, false);

        if (m_LevelSelectionLabel != null)
        {
            Level1DifficultyConfig config = Level1GameSession.GetConfig(m_SelectedDifficulty);
            m_LevelSelectionLabel.text = "Dificuldade escolhida: " + config.displayName + " | " + config.description;
            m_LevelSelectionLabel.color = config.accentColor;
        }
    }

    void ShowSettingsPanel()
    {
        SetPanelState(m_MainPanel, false);
        SetPanelState(m_DifficultyPanel, false);
        SetPanelState(m_LevelPanel, false);
        SetPanelState(m_SettingsPanel, true);
    }

    void SelectDifficulty(Level1Difficulty difficulty)
    {
        m_SelectedDifficulty = difficulty;
        ShowLevelPanel();
    }

    void StartLevel(string sceneName)
    {
        int levelNumber = GetLevelNumberForScene(sceneName);
        if (!CampaignProgression.IsLevelUnlocked(levelNumber))
        {
            if (m_LevelSelectionLabel != null)
            {
                m_LevelSelectionLabel.text = "Nivel " + levelNumber + " bloqueado. Precisas de pelo menos 1 estrela no Nivel " + (levelNumber - 1) + ".";
                m_LevelSelectionLabel.color = new Color(0.93f, 0.46f, 0.35f, 1f);
            }

            return;
        }

        Level1GameSession.RequestLevelStart(m_SelectedDifficulty, sceneName);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    void RefreshDifficultyLabel()
    {
        if (m_CurrentDifficultyLabel == null)
        {
            return;
        }

        Level1DifficultyConfig config = Level1GameSession.currentConfig;
        m_CurrentDifficultyLabel.text = "Dificuldade atual: " + config.displayName;
        m_CurrentDifficultyLabel.color = config.accentColor;
    }

    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        if (m_VolumeValueLabel != null)
        {
            m_VolumeValueLabel.text = Mathf.RoundToInt(value * 100f) + "%";
        }

        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    void LoadVolume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(VolumeKey, 0.85f);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void SetPanelState(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }

    void EnsureEventSystem()
    {
        var eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
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

    GameObject CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        var panelObject = new GameObject(name);
        panelObject.transform.SetParent(parent, false);

        var rectTransform = panelObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        var image = panelObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.11f, 0.14f, 0.92f);

        return panelObject;
    }

    Button CreateMenuButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick)
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
        image.color = new Color(0.15f, 0.2f, 0.25f, 0.98f);

        var button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.23f, 0.31f, 0.38f, 1f);
        colors.pressedColor = new Color(0.1f, 0.14f, 0.18f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(onClick);

        var text = CreateText(
            "Label",
            buttonObject.transform,
            label,
            28,
            TextAnchor.MiddleCenter,
            new Vector2(0f, 0f),
            size,
            new Color(0.97f, 0.98f, 0.99f, 1f));
        text.fontStyle = FontStyle.Bold;

        return button;
    }

    void CreateDifficultyButton(Transform parent, Level1DifficultyConfig config, Vector2 anchoredPosition, Vector2 size)
    {
        var buttonObject = new GameObject(config.displayName + "_DifficultyButton");
        buttonObject.transform.SetParent(parent, false);

        var rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.14f, 0.18f, 0.22f, 0.98f);

        var button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = config.accentColor * 0.9f;
        colors.pressedColor = config.accentColor * 0.75f;
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(() => SelectDifficulty(config.difficulty));

        CreateImage(
            "Accent",
            buttonObject.transform,
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0f),
            new Vector2(12f, 0f),
            config.accentColor);

        var title = CreateText(
            "Title",
            buttonObject.transform,
            config.displayName,
            26,
            TextAnchor.UpperLeft,
            new Vector2(28f, -12f),
            new Vector2(240f, 32f),
            new Color(0.97f, 0.98f, 0.99f, 1f));
        title.fontStyle = FontStyle.Bold;

        CreateText(
            "Description",
            buttonObject.transform,
            config.description,
            18,
            TextAnchor.UpperLeft,
            new Vector2(30f, -44f),
            new Vector2(760f, 40f),
            new Color(0.75f, 0.81f, 0.87f, 1f));

        string loadout = BuildLoadoutDescription(config);
        CreateText(
            "Loadout",
            buttonObject.transform,
            "Arsenal: " + loadout + " | Base: " + config.homeBaseHealth + " HP | Moedas: " + config.startingCurrency,
            17,
            TextAnchor.UpperLeft,
            new Vector2(30f, -66f),
            new Vector2(790f, 24f),
            new Color(0.95f, 0.84f, 0.47f, 1f));
    }

    void CreateLevelButton(Transform parent, string label, string sceneName, Vector2 anchoredPosition, Vector2 size)
    {
        int levelNumber = GetLevelNumberForScene(sceneName);
        bool isUnlocked = CampaignProgression.IsLevelUnlocked(levelNumber);
        int currentStars = CampaignProgression.GetStarsForScene(sceneName);

        var button = CreateMenuButton(
            parent,
            label,
            anchoredPosition,
            size,
            () => StartLevel(sceneName));

        if (!isUnlocked)
        {
            button.interactable = false;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.11f, 0.13f, 0.16f, 0.82f);
            }
        }

        string statusText = isUnlocked
            ? "Estrelas: " + Mathf.Clamp(currentStars, 0, 3)
            : "Bloqueado: precisas de 1 estrela no Nivel " + (levelNumber - 1);

        var statusLabel = CreateText(
            "Status",
            button.transform,
            statusText,
            16,
            TextAnchor.LowerLeft,
            new Vector2(16f, -34f),
            new Vector2(size.x - 24f, 20f),
            isUnlocked
                ? new Color(0.95f, 0.84f, 0.47f, 1f)
                : new Color(0.79f, 0.54f, 0.46f, 1f));
        statusLabel.alignment = TextAnchor.LowerLeft;
    }

    int GetLevelNumberForScene(string sceneName)
    {
        for (int levelNumber = 1; levelNumber <= 5; levelNumber++)
        {
            if (Level1GameSession.GetSceneNameForLevel(levelNumber) == sceneName)
            {
                return levelNumber;
            }
        }

        return 1;
    }

    string BuildLoadoutDescription(Level1DifficultyConfig config)
    {
        string loadout = string.Empty;
        if (config.allowMachineGun)
        {
            loadout = "MachineGun";
        }

        if (config.allowLaser)
        {
            loadout += loadout.Length == 0 ? "Laser" : ", Laser";
        }

        if (config.allowRocket)
        {
            loadout += loadout.Length == 0 ? "Rocket" : ", Rocket";
        }

        return loadout.Length == 0 ? "Sem torres" : loadout;
    }

    Slider CreateSlider(Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        var sliderObject = new GameObject("Slider");
        sliderObject.transform.SetParent(parent, false);

        var rectTransform = sliderObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        var slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        var background = CreateImage(
            "Background",
            sliderObject.transform,
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            new Color(0.17f, 0.22f, 0.26f, 1f));

        var fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(sliderObject.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0f);
        fillAreaRect.anchorMax = new Vector2(1f, 1f);
        fillAreaRect.offsetMin = new Vector2(8f, 5f);
        fillAreaRect.offsetMax = new Vector2(-18f, -5f);

        var fill = CreateImage(
            "Fill",
            fillArea.transform,
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            new Color(0.95f, 0.84f, 0.47f, 1f));

        var handleArea = new GameObject("HandleSlideArea");
        handleArea.transform.SetParent(sliderObject.transform, false);
        var handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = new Vector2(0f, 0f);
        handleAreaRect.anchorMax = new Vector2(1f, 1f);
        handleAreaRect.offsetMin = new Vector2(8f, 0f);
        handleAreaRect.offsetMax = new Vector2(-8f, 0f);

        var handle = CreateImage(
            "Handle",
            handleArea.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(-12f, -12f),
            new Vector2(12f, 12f),
            new Color(0.96f, 0.97f, 0.99f, 1f));

        slider.targetGraphic = handle;
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.direction = Slider.Direction.LeftToRight;
        background.raycastTarget = true;

        return slider;
    }

    Image CreateImage(
        string name,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color)
    {
        var imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);

        var rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;

        var image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    Text CreateText(
        string name,
        Transform parent,
        string value,
        int fontSize,
        TextAnchor alignment,
        Vector2 anchoredPosition,
        Vector2 size,
        Color color)
    {
        var textObject = new GameObject(name);
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
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
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

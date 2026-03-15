using UnityEngine;

public enum Level1Difficulty
{
    Easy,
    Medium,
    Hard,
    Epic
}

public sealed class Level1DifficultyConfig
{
    public readonly Level1Difficulty difficulty;
    public readonly string displayName;
    public readonly string description;
    public readonly float enemyHealthMultiplier;
    public readonly float enemySpeedMultiplier;
    public readonly float enemyCountMultiplier;
    public readonly float rewardMultiplier;
    public readonly float spawnIntervalMultiplier;
    public readonly int startingCurrency;
    public readonly int homeBaseHealth;
    public readonly bool allowMachineGun;
    public readonly bool allowLaser;
    public readonly bool allowRocket;
    public readonly Color accentColor;

    public Level1DifficultyConfig(
        Level1Difficulty difficulty,
        string displayName,
        string description,
        float enemyHealthMultiplier,
        float enemySpeedMultiplier,
        float enemyCountMultiplier,
        float rewardMultiplier,
        float spawnIntervalMultiplier,
        int startingCurrency,
        int homeBaseHealth,
        bool allowMachineGun,
        bool allowLaser,
        bool allowRocket,
        Color accentColor)
    {
        this.difficulty = difficulty;
        this.displayName = displayName;
        this.description = description;
        this.enemyHealthMultiplier = enemyHealthMultiplier;
        this.enemySpeedMultiplier = enemySpeedMultiplier;
        this.enemyCountMultiplier = enemyCountMultiplier;
        this.rewardMultiplier = rewardMultiplier;
        this.spawnIntervalMultiplier = spawnIntervalMultiplier;
        this.startingCurrency = startingCurrency;
        this.homeBaseHealth = homeBaseHealth;
        this.allowMachineGun = allowMachineGun;
        this.allowLaser = allowLaser;
        this.allowRocket = allowRocket;
        this.accentColor = accentColor;
    }
}

public static class Level1GameSession
{
    const string DifficultyKey = "Level1Difficulty";
    public const string MenuSceneName = "MainMenu";
    public const string LevelSceneName = "SampleScene";
    public const string Level2SceneName = "Level2";
    public const string Level3SceneName = "Level3";
    public const string Level4SceneName = "Level4";
    public const string Level5SceneName = "Level5";

    static bool s_HasLoaded;
    static string s_RequestedSceneName;
    static Level1Difficulty s_SelectedDifficulty = Level1Difficulty.Medium;

    public static Level1Difficulty selectedDifficulty
    {
        get
        {
            EnsureLoaded();
            return s_SelectedDifficulty;
        }
    }

    public static Level1DifficultyConfig currentConfig
    {
        get
        {
            EnsureLoaded();
            return GetConfig(s_SelectedDifficulty);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LoadSavedDifficulty()
    {
        EnsureLoaded();
        s_RequestedSceneName = string.Empty;
    }

    public static void SetDifficulty(Level1Difficulty difficulty)
    {
        EnsureLoaded();
        s_SelectedDifficulty = difficulty;
        PlayerPrefs.SetInt(DifficultyKey, (int)difficulty);
        PlayerPrefs.Save();
    }

    public static void RequestLevelStart(Level1Difficulty difficulty, string sceneName)
    {
        SetDifficulty(difficulty);
        s_RequestedSceneName = sceneName;
    }

    public static bool ConsumeLevelStartRequest(string sceneName)
    {
        bool wasRequested = !string.IsNullOrEmpty(sceneName) && s_RequestedSceneName == sceneName;
        s_RequestedSceneName = string.Empty;
        return wasRequested;
    }

    public static string GetSceneNameForLevel(int levelNumber)
    {
        switch (levelNumber)
        {
            case 1:
                return LevelSceneName;
            case 2:
                return Level2SceneName;
            case 3:
                return Level3SceneName;
            case 4:
                return Level4SceneName;
            case 5:
                return Level5SceneName;
            default:
                return LevelSceneName;
        }
    }

    public static Level1DifficultyConfig GetConfig(Level1Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Level1Difficulty.Easy:
                return new Level1DifficultyConfig(
                    difficulty,
                    "Facil",
                    "Mais moedas, todas as torres, inimigos com menos vida.",
                    0.85f,
                    0.95f,
                    0.85f,
                    1.15f,
                    1.06f,
                    900,
                    14,
                    true,
                    true,
                    true,
                    new Color(0.55f, 0.91f, 0.68f, 1f));

            case Level1Difficulty.Hard:
                return new Level1DifficultyConfig(
                    difficulty,
                    "Dificil",
                    "Menos margem para erro e inimigos mais fortes.",
                    1.18f,
                    1.05f,
                    1.15f,
                    0.92f,
                    0.94f,
                    600,
                    9,
                    true,
                    true,
                    true,
                    new Color(0.98f, 0.71f, 0.35f, 1f));

            case Level1Difficulty.Epic:
                return new Level1DifficultyConfig(
                    difficulty,
                    "Epico",
                    "Vida mais alta, menos moedas e arsenal limitado a MachineGun e Rocket.",
                    1.35f,
                    1.08f,
                    1.28f,
                    0.82f,
                    0.88f,
                    500,
                    8,
                    true,
                    false,
                    true,
                    new Color(0.93f, 0.38f, 0.29f, 1f));

            default:
                return new Level1DifficultyConfig(
                    difficulty,
                    "Medio",
                    "Experiencia base do nivel 1.",
                    1f,
                    1f,
                    1f,
                    1f,
                    1f,
                    700,
                    10,
                    true,
                    true,
                    true,
                    new Color(0.48f, 0.77f, 0.95f, 1f));
        }
    }

    static void EnsureLoaded()
    {
        if (s_HasLoaded)
        {
            return;
        }

        s_HasLoaded = true;
        if (PlayerPrefs.HasKey(DifficultyKey))
        {
            s_SelectedDifficulty = (Level1Difficulty)PlayerPrefs.GetInt(DifficultyKey);
        }
    }
}

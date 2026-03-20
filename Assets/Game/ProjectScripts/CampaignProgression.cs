using UnityEngine;

public static class CampaignProgression
{
    const string StarsKeyPrefix = "CampaignStars_";

    public static int GetStarsForScene(string sceneName)
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(GetStarsKey(sceneName), 0));
    }

    public static int GetStarsForLevel(int levelNumber)
    {
        return GetStarsForScene(Level1GameSession.GetSceneNameForLevel(levelNumber));
    }

    public static bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber <= 1)
        {
            return true;
        }

        return GetStarsForLevel(levelNumber - 1) >= 1;
    }

    public static bool IsSceneUnlocked(string sceneName)
    {
        return IsLevelUnlocked(GetLevelNumberForScene(sceneName));
    }

    public static void SetStarsForScene(string sceneName, int stars)
    {
        stars = Mathf.Clamp(stars, 0, 3);
        int existingStars = Mathf.Max(0, GetStarsForScene(sceneName));
        int bestStars = Mathf.Max(existingStars, stars);
        PlayerPrefs.SetInt(GetStarsKey(sceneName), bestStars);
        PlayerPrefs.Save();
    }

    static string GetStarsKey(string sceneName)
    {
        return StarsKeyPrefix + sceneName;
    }

    static int GetLevelNumberForScene(string sceneName)
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
}

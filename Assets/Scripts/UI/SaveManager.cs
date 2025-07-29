using UnityEngine;

public static class SaveManager
{
    private const string UnlockedLevelKey = "UnlockedLevel";
    private const string CurrentLevelKey = "CurrentLevel";

    public static void SaveLevel(int unlockedLevel)
    {
        PlayerPrefs.SetInt(UnlockedLevelKey, unlockedLevel);
        PlayerPrefs.Save();
    }

    public static int LoadLevel()
    {
        return PlayerPrefs.GetInt(UnlockedLevelKey, 1);
    }

    public static void SaveCurrentLevel(int currentLevel)
    {
        PlayerPrefs.SetInt(CurrentLevelKey, currentLevel);
        PlayerPrefs.Save();
    }

    public static int LoadCurrentLevel()
    {
        return PlayerPrefs.GetInt(CurrentLevelKey, 1);
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
    }
}

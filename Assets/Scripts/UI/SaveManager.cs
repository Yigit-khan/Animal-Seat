using UnityEngine;

public static class SaveManager
{
    private const string UnlockedLevelKey = "UnlockedLevel"; //oyuncunun açtýðý en son level
    private const string CurrentLevelKey = "CurrentLevel"; //þu anda oynana level

    //hangi levelde olduðunu kaydediyo
    public static void SaveLevel(int unlockedLevel)
    {
        //unlockedLevel ile veriyi kaydettik
        PlayerPrefs.SetInt(UnlockedLevelKey, unlockedLevel);
        //kaydý diske yazýyor
        PlayerPrefs.Save();
    }

    //açýlmýþ en yüksek level'i yükler
    public static int LoadLevel()
    {
        return PlayerPrefs.GetInt(UnlockedLevelKey, 1);
    }
    //þu anda onynanan leveli kaydettik -- play butonuna basýnca çaðrýlmasý gerekiyor
    public static void SaveCurrentLevel(int currentLevel)
    {
        PlayerPrefs.SetInt(CurrentLevelKey, currentLevel);
        PlayerPrefs.Save();
    }

    //level sahnesindeki uý daki level bilgisini yazabilmek için var
    public static int LoadCurrentLevel()
    {
        return PlayerPrefs.GetInt(CurrentLevelKey, 1);
    }

    //tüm kayýtlarý siliyor
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
    }
}

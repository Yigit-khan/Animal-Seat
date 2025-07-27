using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LevelSelectController : MonoBehaviour
{
    public Text levelDisplayText; //butonun üstündeki text

    private int currentLevelIndex = 1;
    private int unlockedLevel = 1;
    private const int maxLevel = 20;

    private void Start()
    {
        // PlayerPrefs'ten en son geçilen bölümü alýyorum
        unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        currentLevelIndex = unlockedLevel;
        UpdateLevelDisplay();
    }

    //Level isimleri Level 1 þeklinde olmalý
    public void OnPlayButtonPressed()
    {
        Debug.Log("LEVEL " + currentLevelIndex + " sahnesi yükleniyor...");
        SceneManager.LoadScene("Level" + currentLevelIndex); 
    }

    public void SetLevel(int level)
    {
        if (level <= unlockedLevel)
        {
            currentLevelIndex = level;
            UpdateLevelDisplay();
        }
        else
        {
            Debug.Log("Bu level henüz kilitli!");
        }
    }

    //level geçilince çaðrýlacak
    public void UnlockNextLevel()
    {
      
        if (unlockedLevel < maxLevel)
        {
            unlockedLevel++;
            PlayerPrefs.SetInt("UnlockedLevel", unlockedLevel);
            PlayerPrefs.Save();
        }

        Debug.Log("Yeni level açýldý: " + unlockedLevel);
    }

    private void UpdateLevelDisplay()
    {
        levelDisplayText.text = "LEVEL " + currentLevelIndex;
    }

    public int GetCurrentLevel()
    {
        return currentLevelIndex;
    }

    public int GetUnlockedLevel()
    {
        return unlockedLevel;
    }
}

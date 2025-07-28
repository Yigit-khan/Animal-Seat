using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    [Header("UI References")]
    public Text levelDisplayText;      // Play butonunun üstündeki LEVEL X yazýsý
    public Text[] levelNumberText;     // Altýgenlerdeki LEVEL numaralarý (3 tane olmalý)

    private int currentLevelIndex = 1;
    private int unlockedLevel = 1;
    private const int maxLevel = 20;

    private void Start()
    {
        // PlayerPrefs'ten en son geçilen bölümü alýyorum
        unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        currentLevelIndex = unlockedLevel;

        UpdateLevelDisplay();
        UpdateHexNumbers();
    }

    // Play butonuna basýldýðýnda sahneyi yükle
    public void OnPlayButtonPressed()
    {
        Debug.Log("LEVEL " + currentLevelIndex + " sahnesi yükleniyor...");
        SceneManager.LoadScene("Level" + currentLevelIndex);
    }

    // UI veya baþka yerden level seçmek için
    public void SetLevel(int level)
    {
        if (level <= unlockedLevel)
        {
            currentLevelIndex = level;
            UpdateLevelDisplay();
            UpdateHexNumbers();
        }
        else
        {
            Debug.Log("Bu level henüz kilitli!");
        }
    }

    // Level geçildiðinde çaðrýlacak
    public void UnlockNextLevel()
    {
        if (unlockedLevel < maxLevel)
        {
            unlockedLevel++;
            PlayerPrefs.SetInt("UnlockedLevel", unlockedLevel);
            PlayerPrefs.Save();
        }

        Debug.Log("Yeni level açýldý: " + unlockedLevel);

        // Hex numaralarýný güncelle
        UpdateHexNumbers();
    }

    private void UpdateLevelDisplay()
    {
        levelDisplayText.text = "LEVEL " + currentLevelIndex;
    }

    private void UpdateHexNumbers()
    {
        for (int i = 0; i < levelNumberText.Length; i++)
        {
            int displayedLevel = currentLevelIndex + i;

            if (displayedLevel <= maxLevel)
                levelNumberText[i].text = displayedLevel.ToString();
            else
                levelNumberText[i].text = "-"; // Max level sonrasý boþ göster
        }
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

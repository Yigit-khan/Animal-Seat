using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text levelDisplayText;
    public TMP_Text[] levelNumberText;

    private int currentLevelIndex = 1;
    [SerializeField]
    private int unlockedLevel = 1;
    private const int maxLevel = 20;

    private void Start()
    {
        // SaveManager üzerinden veri yükle
        unlockedLevel = SaveManager.LoadLevel();
        currentLevelIndex = unlockedLevel;

        UpdateLevelDisplay();
        UpdateHexNumbers();
    }

    public void OnPlayButtonPressed()
    {
        SaveManager.SaveCurrentLevel(currentLevelIndex);  // Sadece seçili level kaydedilir
        string sceneName = "Level" + currentLevelIndex;
        Debug.Log("Yükleniyor: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void SetLevel(int level)
    {
        if (level <= unlockedLevel)
        {
            currentLevelIndex = level;
            SaveManager.SaveCurrentLevel(currentLevelIndex); // Level seçildiðinde kaydet
            UpdateLevelDisplay();
            UpdateHexNumbers();
            Debug.Log("Seçilen Level: " + level);
        }
        else
        {
            Debug.Log("Bu level henüz kilitli!");
        }
    }

    public void UnlockNextLevel()
    {
        if (unlockedLevel < maxLevel)
        {
            unlockedLevel++;
            SaveManager.SaveLevel(unlockedLevel);
        }

        Debug.Log("Yeni level açýldý: " + unlockedLevel);
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
                levelNumberText[i].text = "-";
        }
    }
}

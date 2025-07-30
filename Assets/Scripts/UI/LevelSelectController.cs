using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text levelDisplayText; //butonun üstündeki text
    public TMP_Text[] levelNumberText; //altýgenlerin üstündeki textler

    private int currentLevelIndex = 1; //þu anki leveli tutuyor
    [SerializeField]
    private int unlockedLevel = 1; //henü açýlmamýþ leveli tutuyor
    private const int maxLevel = 20; //þimdilik böyle

    private void Start()
    {
        // SaveManager üzerinden veri yükle -- en son kaldýðýmýz level yükleniyor
        unlockedLevel = SaveManager.LoadLevel();
        currentLevelIndex = unlockedLevel;

        //butonun ve altýgenlerin üstündeki yazýlar güncelleniyor
        UpdateLevelDisplay();
        UpdateHexNumbers();
    }
    
    //menüdeki play butonuna basýnca çalýþýyor
    public void OnPlayButtonPressed()
    {
        //en son geldiðimiz leveli atadýk
        currentLevelIndex = unlockedLevel;
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

    //bu fonksiyon level baþarýyla geçilince çaðrýlacak
    public void UnlockNextLevel()
    {
        if (unlockedLevel < maxLevel)
        {
            unlockedLevel++;
            SaveManager.SaveLevel(unlockedLevel);

            // En son açýlan level seçili olsun
            currentLevelIndex = unlockedLevel;

            // UI güncelle
            UpdateLevelDisplay();
            UpdateHexNumbers();
        }

        Debug.Log("Yeni level açýldý: " + unlockedLevel);
    }

    //butonun üstündeki yazýyý güncelliyor
    private void UpdateLevelDisplay()
    {
        levelDisplayText.text = "LEVEL " + currentLevelIndex;
    }

    //altýgenlerin üstündeki yazýlarý güncelliyor
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

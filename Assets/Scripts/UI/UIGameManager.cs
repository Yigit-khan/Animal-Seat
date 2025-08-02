using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UIGameManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPopUp;
    public GameObject replayPopUp;
    public GameObject darkBackground;

    [Header("Buttons")]
    public Button settingsButton;
    public Button ReplayButton;
    public Button coinPlusButton;
    public Button exitButtonSettings;
    [Tooltip("Ayarlar popup'ýndaki 'Ana Menü' butonu")]
    public Button mainMenuExitButton; // Yeni buton referans

    [Header("Replay Popup Buttons")]
    [Tooltip("Popup'taki 'Stay' butonu")]
    public Button buttonStay;
    [Tooltip("Popup'taki 'Restart' butonu")]
    public Button buttonRestart;

    [Header("Texts")]
    public TMP_Text levelInfo;

    //public LevelSelectController levelSelectController;

    private void Start()
    {

        settingsButton.onClick.AddListener(OpensettingsPopUp);
        ReplayButton.onClick.AddListener(OpenReplayPopUp);
        exitButtonSettings.onClick.AddListener(CloseSettingsPopUp);

        if (buttonStay != null)
            buttonStay.onClick.AddListener(CloseReplayPopUp); // Stay butonu sadece popup'ý kapatýr.

        if (buttonRestart != null)
            buttonRestart.onClick.AddListener(OnRestartButtonPressed); // Restart butonu seviyeyi yeniden baþlatýr.

        if (mainMenuExitButton != null)
            mainMenuExitButton.onClick.AddListener(GoToMainMenu);

        int currentLevel = SaveManager.LoadCurrentLevel();
        levelInfo.text = "Level " + currentLevel;
    }


    public void OpensettingsPopUp()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");

        GameManager.Instance.PauseGame(); //oyunu duraklat

        settingsPopUp.SetActive(true);
        darkBackground.SetActive(true);
    }

    public void OpenReplayPopUp()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");

        GameManager.Instance.PauseGame();  //oyunu duraklat

        replayPopUp.SetActive(true);
        darkBackground.SetActive(true);
    }

    public void CloseSettingsPopUp()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");

        GameManager.Instance.ResumeGame();

        settingsPopUp.SetActive(false);
        darkBackground.SetActive(false);
    }

    public void CloseReplayPopUp()
    {
        GameManager.Instance.ResumeGame();

        replayPopUp.SetActive(false);
        darkBackground.SetActive(false);
    }

    public void OnRestartButtonPressed()
    {
        
        // Görevi GameManager'a devret.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartCurrentLevel();
        }
        else
        {
            Debug.LogError("GameManager.Instance bulunamadý! Seviye yeniden baþlatýlamýyor.");
        }
        
    }

    public void GoToMainMenu()
    {
        // Ses efekti çal (isteðe baðlý)
        SoundManager.Instance.PlaySFX("ButtonClick");

        // Oyunu durdurmuþ olabilecek herhangi bir durumu normale döndür.
        Time.timeScale = 1f;

        // "MenuScene" adlý sahneyi yükle. Sahne adýnýn doðru olduðundan emin ol.
        SceneManager.LoadScene("MenuScene");
    }

}

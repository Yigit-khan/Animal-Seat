using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    // --- YENÝ BUTONLAR ---
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

        int currentLevel = SaveManager.LoadCurrentLevel();
        levelInfo.text = "Level " + currentLevel;
    }


    public void OpensettingsPopUp()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");

        settingsPopUp.SetActive(true);
        darkBackground.SetActive(true);
    }

    public void OpenReplayPopUp()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");

        replayPopUp.SetActive(true);
        darkBackground.SetActive(true);
    }

    public void CloseSettingsPopUp()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");

        settingsPopUp.SetActive(false);
        darkBackground.SetActive(false);
    }

    public void CloseReplayPopUp()
    {
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

}

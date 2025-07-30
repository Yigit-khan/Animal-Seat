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

    [Header("Texts")]
    public TMP_Text levelInfo;

    public LevelSelectController levelSelectController;

    private void Start()
    {

        settingsButton.onClick.AddListener(OpensettingsPopUp);
        ReplayButton.onClick.AddListener(OpenReplayPopUp);
        exitButtonSettings.onClick.AddListener(CloseSettingsPopUp);

        int currentLevel = SaveManager.LoadCurrentLevel();
        levelInfo.text = "Level " + currentLevel;
    }


    public void OpensettingsPopUp()
    {
        settingsPopUp.SetActive(true);
        darkBackground.SetActive(true);
    }

    public void OpenReplayPopUp()
    {
        replayPopUp.SetActive(true);
        darkBackground.SetActive(true);
    }

    public void CloseSettingsPopUp()
    {
        settingsPopUp.SetActive(false);
        darkBackground.SetActive(false);
    }

}

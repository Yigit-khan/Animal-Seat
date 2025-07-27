using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager: MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPopUp;
    public GameObject lifePopUp;
    public GameObject darkBackground;

    [Header("Buttons")]
    public Button settingsButton;
    public Button lifePlusButton;
    public Button coinPlusButton; //þimdilik boþ
    public Button exitButtonSettings;
    public Button exitButtonLife;

    [SerializeField]
    private int lifeCount = 3;

    [Header("Texts")]
    public Text lifeText;
    public Text lifeTimerText;

    private float lifeCoolDown = 1800f; //sayaç için 30dk
    private Coroutine lifeCoroutine;
    private void Start()
    {
        settingsButton.onClick.AddListener(OpensettingsPopUp);
        lifePlusButton.onClick.AddListener(OpenLifePopUp);

        exitButtonSettings.onClick.AddListener(CloseSettingsPopUp);
        exitButtonLife.onClick.AddListener(CloseLifePopUp);

        //oyunun baþýnda zamanlayýcýyý baþlatmak amaçlý
        if(lifeCount < 5 && lifeCoroutine == null)
        {
            lifeCoroutine = StartCoroutine(LifeTimerCoroutine());
        }
    }

    public void OpensettingsPopUp()
    {
        settingsPopUp.SetActive(true);
        darkBackground.SetActive(true);
    }

    public void OpenLifePopUp()
    {
        lifePopUp.SetActive(true);
        darkBackground.SetActive(true);
    }

    public void CloseSettingsPopUp()
    {
        settingsPopUp.SetActive(false);
        darkBackground.SetActive(false);
    }

    public void CloseLifePopUp()
    {
        lifePopUp.SetActive(false);
        darkBackground.SetActive(false);
    }

    void AddLife()
    {
        if (lifeCount < 5)
        {
            lifeCount++;
            UpdateLifeUI();

            if (lifeCount == 5 && lifeCoroutine != null)
            {
                StopCoroutine(lifeCoroutine);
                lifeCoroutine = null;
            }
        }
    }

    void UpdateLifeUI()
    {
        if (lifeCount >= 5)
        {
            lifeText.text = "Full";
            lifeTimerText.text = "";
        }
        else
        {
            lifeText.text = lifeCount.ToString();
        }
    }

    IEnumerator LifeTimerCoroutine()
    {
        while (lifeCount < 5)
        {
            float remaining = lifeCoolDown;
            while (remaining > 0)
            {
                TimeSpan t = TimeSpan.FromSeconds(remaining);
                lifeTimerText.text = $"{t.Minutes:D2}:{t.Seconds:D2}";
                yield return new WaitForSeconds(1f);
                remaining -= 1f;
            }

            lifeCount++;
            UpdateLifeUI();
        }

        lifeTimerText.text = "Full";
        lifeCoroutine = null;
    }
}

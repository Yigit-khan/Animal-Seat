using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPopUp;
    public GameObject lifePopUp;
    public GameObject darkBackground;
    public GameObject leaderBoardPanel;
    public GameObject shopPanel;

    [Header("Buttons")]
    public Button settingsButton;
    public Button lifePlusButton;
    public Button coinPlusButton;
    public Button exitButtonSettings;
    public Button exitButtonLife;
    public Button cupButton;
    public Button homeButton;
    public Button shopButton;

    [SerializeField]
    private int lifeCount = 3; //denemek için böyle verdim

    [Header("Texts")]
    public TMP_Text lifeText;
    public TMP_Text lifeTimerText;
    public TMP_Text coinText;

    private float lifeCoolDown = 1800f; //sayaç için 30dk
    private Coroutine lifeCoroutine;
    private CoinManager _coinManager;


    public CanvasGroup fadePanel; // Fade için CanvasGroup
    public float fadeDuration = 0.5f; // Kararma süresi

    private void Start()
    {
        //butonlara fonksiyon atamasý
        settingsButton.onClick.AddListener(OpensettingsPopUp);
        lifePlusButton.onClick.AddListener(OpenLifePopUp);
        coinPlusButton.onClick.AddListener(OpenShopPanel);

        exitButtonSettings.onClick.AddListener(CloseSettingsPopUp);
        exitButtonLife.onClick.AddListener(CloseLifePopUp);

        homeButton.onClick.AddListener(OpenHome);
        cupButton.onClick.AddListener(OpenLeaderBoard);
        shopButton.onClick.AddListener(OpenShopPanel);

        _coinManager = CoinManager.Instance;

        UpdateLifeUI();
        UpdateCoins();

        // Oyun baþýnda zamanlayýcýyý baþlatýyoruz
        if (lifeCount < 5 && lifeCoroutine == null)
        {
            lifeCoroutine = StartCoroutine(LifeTimerCoroutine());
        }
    }


    public void OpensettingsPopUp()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");

        settingsPopUp.SetActive(true);
        darkBackground.SetActive(true);
    }

    public void OpenLifePopUp()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");

        lifePopUp.SetActive(true);
        darkBackground.SetActive(true);
    }

    public void CloseSettingsPopUp()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");

        settingsPopUp.SetActive(false);
        darkBackground.SetActive(false);
    }

    public void CloseLifePopUp()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");

        lifePopUp.SetActive(false);
        darkBackground.SetActive(false);
    }

    public void OpenHome()
    {
        StartCoroutine(FadeOutAndHideLeaderboardandShop());
    }

    public void OpenLeaderBoard()
    {
        StartCoroutine(FadeInAndShowLeaderboard());
    }
    public void OpenShopPanel()
    {
        StartCoroutine(FadeInAndShowShopPanel());
    }
    IEnumerator FadeInAndShowLeaderboard()
    {
        // Kararma
        float t = 0;
        fadePanel.gameObject.SetActive(true);
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        leaderBoardPanel.SetActive(true);
        shopPanel.SetActive(false);
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        fadePanel.gameObject.SetActive(false);
    }

    IEnumerator FadeInAndShowShopPanel()
    {
        // Kararma
        float t = 0;
        fadePanel.gameObject.SetActive(true);
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        shopPanel.SetActive(true);
        leaderBoardPanel.SetActive(false);
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        fadePanel.gameObject.SetActive(false);
    }
    IEnumerator FadeOutAndHideLeaderboardandShop()
    {
        // Kararma (fade in)
        float t = 0;
        fadePanel.gameObject.SetActive(true);
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }


        leaderBoardPanel.SetActive(false);
        shopPanel.SetActive(false);

        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        fadePanel.gameObject.SetActive(false);
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
            lifeText.text = "5/5";
            lifeTimerText.text = "";
        }
        else
        {
            lifeText.text = lifeCount.ToString() + "/5";
        }
    }

    void UpdateCoins()
    {
        coinText.text = _coinManager.CurrentCoins.ToString();
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

        lifeTimerText.text = "00:00";
        lifeCoroutine = null;
    }




}

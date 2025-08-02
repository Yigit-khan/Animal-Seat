using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIAnimationManager : MonoBehaviour
{

    [Header("Coin Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private RectTransform coinSpawnOrigin;
    [SerializeField] private RectTransform coinTarget;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private int coinRewardAmount = 100;

    [Header("Win UI Buttons")]
    [SerializeField] private Button buttonContinue;
    [SerializeField] private Button button2xContinue;
    private int totalCoins = 0;

    [Header("Lose UI Buttons")]
    [SerializeField] private Button buttonRetry;
    [SerializeField] private Button buttonExit;

    [Header("UI Text References")]
    [Tooltip("Kaybetme ekranýnda gösterilecek ana baþlýk (FAILED / NO MOVES LEFT)")]
    [SerializeField] private TMP_Text loseTitleText; // YENÝ

    private CoinManager _coinManager;

    [Header("Silinecek")]
    //Silinecek
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip coinCollectSound;

    void Start()
    {
        _coinManager = CoinManager.Instance;
        coinText.text = coinRewardAmount.ToString();
        buttonContinue.onClick.AddListener(() => OnContinueClicked(false));
        button2xContinue.onClick.AddListener(() => OnContinueClicked(true));

        // Butonlara týklandýðýnda hangi fonksiyonlarýn çalýþacaðýný ata.
        if (buttonRetry != null)
            buttonRetry.onClick.AddListener(OnRetryButtonPressed);

        if (buttonExit != null)
            buttonExit.onClick.AddListener(GoToMenu);
    }

    private void OnRetryButtonPressed()
    {
        // Tekrar týklamayý önlemek için butonlarý devre dýþý býrak.
        buttonContinue.interactable = false;
        buttonExit.interactable = false;

        // Önce reklam göster, reklam bittiðinde ödülü ver.
        ShowRewardedAd(() =>
        {
            // --- ÖDÜL KISMI ---
            // 1. GameManager'ý bul ve 1 can eklemesini söyle.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddOneLife();
            }

            // 2. Oyunu normale döndür ve level'ý yeniden baþlat.
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    public void SetupLoseScreen(string title)
    {
        if (loseTitleText != null)
        {
            loseTitleText.text = title;
        }
    }
    public void GoToMenu()
    {
        // Oyunu normale döndür ve menüye git.
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }

    public void OnContinueClicked(bool isDouble)
    {
        buttonContinue.interactable = false;
        button2xContinue.interactable = false;

        if (isDouble)
        {
            ShowRewardedAd(() =>
            {
                // Oyuncu reklamý izledi, þimdi ekstra ödülü ver.
                // Zaten 100 coin'i kazanmýþtý, þimdi bir 100 daha ekliyoruz.
                _coinManager.AddCoins(coinRewardAmount);
                // Animasyonu 200 coin için göster.
                CollectCoinsAndProceed(coinRewardAmount * 2);
            });
        }
        else
        {
            // Normal devam etme. Sadece animasyonu 100 coin için göster.
            CollectCoinsAndProceed(coinRewardAmount);
        }
    }

    private void CollectCoinsAndProceed(int amount)
    {
        /* gamemanagera taþýndý
        _coinManager.AddCoins(amount);
        Debug.Log("Current coins: " + _coinManager.CurrentCoins);
        */


        /* Burasý gamemanager içerisinde taþýnacak
        // 1. Yeni seviye kilidini açma mantýðýný BURAYA TAÞIYIN
        int currentLevel = SaveManager.LoadCurrentLevel();
        int unlockedLevel = SaveManager.LoadLevel();

        // Eðer bitirdiðimiz seviye, en son açýlan seviyeye eþitse, bir sonrakini aç.
        if (currentLevel >= unlockedLevel)
        {
            SaveManager.SaveLevel(currentLevel + 1);
            Debug.Log($"Yeni seviye açýldý: {currentLevel + 1}");
        }
        */

        // 2. Coin animasyonunu baþlat
        int visualCoinCount = Mathf.Min(20, amount);
        float moveTime = 1f;
        float delayStep = 0.05f;

        // Eðer hiç coin gösterilmeyecekse direkt menüye dön.
        if (visualCoinCount == 0)
        {
            SceneManager.LoadScene("MenuScene");
            return;
        }

        for (int i = 0; i < visualCoinCount; i++)
        {
            // ... (Mevcut coin animasyon kodunuz burada kalabilir)
            GameObject coin = Instantiate(coinPrefab, coinSpawnOrigin.transform.parent);
            coin.transform.position = coinSpawnOrigin.position;
            Vector3 initialScale = Vector3.zero;
            Vector3 punchScale = Vector3.one * 1.5f;
            Vector3 finalScale = Vector3.one * 0.4f;
            Vector2 randomOffset = Random.insideUnitCircle.normalized * 60f;
            Vector3 spreadPosition = coinSpawnOrigin.position + new Vector3(randomOffset.x, randomOffset.y + 50f, 0);
            coin.transform.localScale = initialScale;
            float delay = i * delayStep;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.Append(coin.transform.DOScale(punchScale, 0.3f).SetEase(Ease.OutBack));
            seq.Join(coin.transform.DOMove(spreadPosition, 0.3f).SetEase(Ease.OutQuad));
            seq.Append(coin.transform.DOMove(coinTarget.position, moveTime).SetEase(Ease.InQuad));
            seq.Join(coin.transform.DOScale(finalScale, moveTime));
            seq.OnComplete(() => Destroy(coin));

            DOVirtual.DelayedCall(delay + 0.3f, () => audioSource.PlayOneShot(coinCollectSound));

            // 3. SADECE SON coin animasyonu bittiðinde menüye dön
            if (i == visualCoinCount - 1)
            {
                seq.OnComplete(() =>
                {
                    Destroy(coin);
                    // Animasyon bitti, þimdi menüye dönebiliriz.
                    SceneManager.LoadScene("MenuScene");
                });
            }
        }

        // TODO: totalCoins'i PlayerPrefs ile kaydetmelisiniz.
        // int savedCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        // PlayerPrefs.SetInt("TotalCoins", savedCoins + amount);
        totalCoins += amount;
        Debug.Log("Total Coins: " + totalCoins);
    }

    private void ShowRewardedAd(System.Action onComplete)
    {
        Debug.Log("Reklam gösteriliyor...");
        StartCoroutine(SimulateAd(onComplete));
    }

    private IEnumerator SimulateAd(System.Action onComplete)
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("Reklam bitti!");
        onComplete?.Invoke();
    }
}

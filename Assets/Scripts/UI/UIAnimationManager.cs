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

    [Header("Coin Animation Settings")]
    [SerializeField] private float coinMoveTime = 0.5f;       // Faster move to target (was 1.0)
    [SerializeField] private float coinBurstDuration = 0.2f;  // Faster spawn/spread burst (was 0.3)
    [SerializeField] private float coinDelayStep = 0.03f;     // Tighter stagger between coins (was 0.05)

    [Header("Win UI Buttons")]
    [SerializeField] private Button buttonContinue;
    [SerializeField] private Button button2xContinue;
    private int totalCoins = 0;

    [Header("Lose UI Buttons")]
    [SerializeField] private Button buttonRetry;
    [SerializeField] private Button buttonExit;

    [Header("UI Text References")]
    [Tooltip("Kaybetme ekran�nda g�sterilecek ana ba�l�k (FAILED / NO MOVES LEFT)")]
    [SerializeField] private TMP_Text loseTitleText; // YEN�

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

        // Butonlara t�kland���nda hangi fonksiyonlar�n �al��aca��n� ata.
        if (buttonRetry != null)
            buttonRetry.onClick.AddListener(OnRetryButtonPressed);

        if (buttonExit != null)
            buttonExit.onClick.AddListener(GoToMenu);
    }

    private void OnRetryButtonPressed()
    {
        // Tekrar t�klamay� �nlemek i�in butonlar� devre d��� b�rak.
        buttonContinue.interactable = false;
        buttonExit.interactable = false;

        // �nce reklam g�ster, reklam bitti�inde �d�l� ver.
        ShowRewardedAd(() =>
        {
            // --- �D�L KISMI ---
            // 1. GameManager'� bul ve 1 can eklemesini s�yle.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
                GameManager.Instance.AddOneLife();
            }

            // 2. Oyunu normale d�nd�r ve level'� yeniden ba�lat.
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
        // Oyunu normale d�nd�r ve men�ye git.
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
                // Oyuncu reklam� izledi, �imdi ekstra �d�l� ver.
                // Zaten 100 coin'i kazanm��t�, �imdi bir 100 daha ekliyoruz.
                _coinManager.AddCoins(coinRewardAmount);
                // Animasyonu 200 coin i�in g�ster.
                CollectCoinsAndProceed(coinRewardAmount * 2);
            });
        }
        else
        {
            // Normal devam etme. Sadece animasyonu 100 coin i�in g�ster.
            CollectCoinsAndProceed(coinRewardAmount);
        }
    }

    private void CollectCoinsAndProceed(int amount)
    {
        /* gamemanagera ta��nd�
        _coinManager.AddCoins(amount);
        Debug.Log("Current coins: " + _coinManager.CurrentCoins);
        */


        /* Buras� gamemanager i�erisinde ta��nacak
        // 1. Yeni seviye kilidini a�ma mant���n� BURAYA TA�IYIN
        int currentLevel = SaveManager.LoadCurrentLevel();
        int unlockedLevel = SaveManager.LoadLevel();

        // E�er bitirdi�imiz seviye, en son a��lan seviyeye e�itse, bir sonrakini a�.
        if (currentLevel >= unlockedLevel)
        {
            SaveManager.SaveLevel(currentLevel + 1);
            Debug.Log($"Yeni seviye a��ld�: {currentLevel + 1}");
        }
        */

        // 2. Coin animasyonunu ba�lat
        int visualCoinCount = Mathf.Min(20, amount);

        // E�er hi� coin g�sterilmeyecekse direkt men�ye d�n.
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
            float delay = i * coinDelayStep;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.Append(coin.transform.DOScale(punchScale, coinBurstDuration).SetEase(Ease.OutBack));
            seq.Join(coin.transform.DOMove(spreadPosition, coinBurstDuration).SetEase(Ease.OutQuad));
            seq.Append(coin.transform.DOMove(coinTarget.position, coinMoveTime).SetEase(Ease.InQuad));
            seq.Join(coin.transform.DOScale(finalScale, coinMoveTime));
            seq.OnComplete(() => Destroy(coin));

            DOVirtual.DelayedCall(delay + coinBurstDuration, () => audioSource.PlayOneShot(coinCollectSound));

            // 3. SADECE SON coin animasyonu bitti�inde men�ye d�n
            if (i == visualCoinCount - 1)
            {
                seq.OnComplete(() =>
                {
                    Destroy(coin);
                    // Animasyon bitti, �imdi men�ye d�nebiliriz.
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
        Debug.Log("Reklam g�steriliyor...");
        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame();
        StartCoroutine(SimulateAd(onComplete));
    }

    private IEnumerator SimulateAd(System.Action onComplete)
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("Reklam bitti!");
        onComplete?.Invoke();
    }
}

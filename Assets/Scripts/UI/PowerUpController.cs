using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // DOTween kütüphanesini kullanmak için bu satýr gereklidir.

/// <summary>
/// "Geri Alma" power-up'ýnýn UI elemanlarýný, sayýsýný ve animasyonlarýný yönetir.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    public static PowerUpController Instance;

    [Header("UI Referanslarý")]
    [Tooltip("Geri alma power-up'ýný tetikleyen buton.")]
    [SerializeField] private Button recallButton;
    [Tooltip("Kalan power-up sayýsýný gösteren TextMeshPro metni.")]
    [SerializeField] private TMP_Text recallCountText;

    [Header("Ayarlar")]
    [Tooltip("Oyuncunun her seviye baþýnda sahip olacaðý geri alma hakký sayýsý.")]
    [SerializeField] private int startingRecallCount = 2;

    [Header("Animasyon Ayarlarý")]
    [Tooltip("Mod aktifken butonun ne kadar büyüyeceði (1.2 = %120).")]
    [SerializeField] private float buttonScaleAmount = 1.2f;
    [Tooltip("Butonun büyüme/küçülme animasyonunun saniye cinsinden süresi.")]
    [SerializeField] private float buttonAnimationDuration = 0.3f;

    // --- Özel Deðiþkenler ---
    private int recallPowerUpCount;
    private Vector3 initialButtonScale; // Butonun orijinal boyutunu animasyon sonrasý geri dönmek için saklar.

    private void Awake()
    {
        // Singleton Deseni: Sahnede sadece bir tane PowerUpController olmasýný saðlar.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Baþlangýç deðerlerini ata ve UI'ý güncelle.
        recallPowerUpCount = startingRecallCount;
        UpdateUI();

        // Butonun týklama olayýna ilgili fonksiyonu ata.
        if (recallButton != null)
        {
            recallButton.onClick.AddListener(OnRecallButtonClick);
            // Butonun baþlangýçtaki orijinal boyutunu kaydet.
            initialButtonScale = recallButton.transform.localScale;
        }
    }

    /// <summary>
    /// Geri alma butonu týklandýðýnda GameManager'daki ilgili fonksiyonu tetikler.
    /// </summary>
    private void OnRecallButtonClick()
    {
        GameManager.Instance.ActivateRecallMode();
    }

    /// <summary>
    /// Geri alma modunun aktif olup olmadýðýný UI'da görsel olarak ayarlar (buton animasyonu).
    /// </kýsayol>
    public void SetRecallModeActiveVisuals(bool isActive)
    {
        if (recallButton == null) return;

        // Olasý çakýþmalarý önlemek için önceki animasyonlarý durdur.
        recallButton.transform.DOKill();

        if (isActive)
        {
            // Butonu DOTween ile büyüt.
            recallButton.transform.DOScale(initialButtonScale * buttonScaleAmount, buttonAnimationDuration)
                .SetEase(Ease.OutBack); // Animasyona canlýlýk katan bir efekt.
        }
        else
        {
            // Butonu DOTween ile orijinal boyutuna geri döndür.
            recallButton.transform.DOScale(initialButtonScale, buttonAnimationDuration)
                .SetEase(Ease.OutBack);
        }
    }

    /// <summary>
    /// Bir power-up kullanýldýðýnda bu fonksiyon çaðrýlýr. Sayýyý düþürür ve UI'ý günceller.
    /// </summary>
    public void UsePowerUp()
    {
        if (recallPowerUpCount > 0)
        {
            recallPowerUpCount--;
            UpdateUI();
        }
    }

    /// <summary>
    /// Oyuncuya belirtilen miktarda power-up ekler (Dükkan vb. için).
    /// </summary>
    public void AddPowerUps(int amount)
    {
        recallPowerUpCount += amount;
        UpdateUI();
    }

    /// <summary>
    /// Mevcut power-up sayýsýný döndürür.
    /// </summary>
    public int GetPowerUpCount()
    {
        return recallPowerUpCount;
    }

    /// <summary>
    /// Kalan power-up sayýsýný gösteren metni günceller.
    /// </summary>
    private void UpdateUI()
    {
        if (recallCountText != null)
        {
            recallCountText.text = recallPowerUpCount.ToString();
        }
    }
}
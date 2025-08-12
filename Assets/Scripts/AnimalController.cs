using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bir hayvanın temel davranışlarını, durumunu ve verilerini yönetir.
/// Animator'deki 'isSeated' parametresini otomatik olarak günceller.
/// </summary>
public class AnimalController : MonoBehaviour
{
    public static readonly List<AnimalController> Instances = new List<AnimalController>();
    private void OnEnable() => Instances.Add(this);
    private void OnDisable() => Instances.Remove(this);

    [Header("Veri Referansı")]
    [Tooltip("Bu hayvanın tüm verilerini ve karakteristiklerini tutan ScriptableObject.")]
    public AnimalSO animalSO;
    public Renderer eyepatchRenderer;

    [Header("Durum Değişkenleri")]
    [Tooltip("Bu hayvanın 'Geri Alma' power-up'ı ile geri çağrılıp çağrılamayacağını belirtir.")]
    public bool isRecallable = true;

    // --- ÖZEL DEĞİŞKENLER ---
    public int originalLayer { get; private set; }
    public List<SeatController> occupiedSeats = new List<SeatController>();
    private List<GameObject> myBubbles = new List<GameObject>();
    private Animator animator; // Animator bileşenini hafızada tutmak için (performans).
    private bool _isSeated = false; // "isSeated" durumunu içeride saklamak için özel değişken.

    private Renderer[] animalRenderers;
    private List<Color> originalColors = new List<Color>();

    [Header("Oturma Animasyon Ayarları")]
    [Tooltip("Otururken oynatılacak rastgele animasyonun Animator'deki trigger adı.")]
    [SerializeField] private string idleAnimationTriggerName = "IdleAction";
    [Tooltip("Rastgele animasyonlar arasındaki minimum bekleme süresi (saniye).")]
    [SerializeField] private float minIdleInterval = 5.0f;
    [Tooltip("Rastgele animasyonlar arasındaki maksimum bekleme süresi (saniye).")]
    [SerializeField] private float maxIdleInterval = 15.0f;
    private Coroutine idleAnimationCoroutine; // Rastgele animasyon döngüsünü tutmak için Coroutine referansı.

    private void Awake()
    {
        if (animalSO != null)
        {
            animalSO = Instantiate(animalSO);
        }
        else
        {
            Debug.LogError($"[{name}] animalSO asset’i atanmamış!");
        }



    }

    /// <summary>
    /// Hayvanın oturup oturmadığını yönetir. Değeri her değiştiğinde Animator'ü otomatik olarak günceller.
    /// </summary>
    public bool isSeated
    {
        get { return _isSeated; }
        set
        {
            if (_isSeated == value) return;
            _isSeated = value;

            if (animator != null)
            {
                animator.SetBool("isSeated", _isSeated);
            }

            // --- YENİ MANTIK ---
            // Eğer hayvan OTURUYORSA, rastgele animasyon döngüsünü BAŞLAT.
            if (_isSeated)
            {
                StartIdleAnimationRoutine();
            }
            // Eğer hayvan artık oturmuyorsa (kalkıyorsa), rastgele animasyon döngüsünü DURDUR.
            else
            {
                StopIdleAnimationRoutine();
            }
        }
    }

    /// <summary>
    /// Hayvan oluşturulduğunda veya oyuna dahil edildiğinde çağrılır.
    /// </summary>
    public void Initialize()
    {
        originalLayer = gameObject.layer;

        // Animator bileşenini en başta bir kere bulup hafızaya alalım.
        animator = GetComponent<Animator>();

        animalRenderers = GetComponentsInChildren<Renderer>(true);

        foreach (var rend in animalRenderers)
        {
            // Materyalin bir kopyasını oluşturduğumuzdan emin olalım ki diğer hayvanları etkilemesin.
            originalColors.Add(rend.material.color);
            if (rend.name == "eyepatch")
                eyepatchRenderer = rend;
        }

        // --- DEBUG 4: Initialize çağrıldığını ve Animator'ün durumunu logla. ---
        if (animator != null)
        {
            Debug.Log($"<color=green>[{this.name}]</color> Initialize edildi. Animator bulundu.");
            animator.SetBool("isSeated", false); // Başlangıç durumunu ayarla.
        }
        else
        {
            Debug.LogError($"<color=red>[{this.name}]</color> Initialize edildi ama Animator BULUNAMADI.");
        }
    }

    public void PlayErrorFeedback()
    {
        if (animalRenderers == null || animalRenderers.Length == 0) return;

        // Hayvanın ana gövdesini hafifçe titret.
        transform.DOShakePosition(duration: 0.5f, strength: 0.1f, vibrato: 20);

        // Hayvanın tüm görsel parçalarını 0.15 saniyede kırmızı yap,
        // bir süre bekle, sonra 0.3 saniyede eski rengine geri döndür.
        for (int i = 0; i < animalRenderers.Length; i++)
        {
            int index = i; // Döngü içinde lambda kullanırken closure problemi yaşamamak için.

            Sequence feedbackSequence = DOTween.Sequence();
            feedbackSequence.Append(animalRenderers[index].material.DOColor(Color.red, 0.15f));
            feedbackSequence.AppendInterval(0.2f); // Bu süre kadar kırmızı kalacak.
            feedbackSequence.Append(animalRenderers[index].material.DOColor(originalColors[index], 0.3f));
        }
    }

    /// <summary>
    /// Bu hayvanın sahip olduğu tüm karakteristikler (trait) için düşünce balonları oluşturur.
    /// </summary>
    public void DisplayMyRules()
    {
        // Eğer hayvan "oturuyor" olarak işaretlenmişse, ASLA balon oluşturma.
        if (isSeated)
        {
            ClearMyBubbles();
            return;
        }

        ClearMyBubbles();

        if (animalSO == null || animalSO.traits == null) return;

        // Balon oluşturma mantığı
        foreach (var trait in animalSO.traits)
        {
            var icon = trait.traitIcon;
            if (icon != null)
            {
                GameObject bubbleObj = GameManager.Instance.CreateThoughtBubble(icon, trait.traitDescription);
                if (bubbleObj != null)
                {
                    bubbleObj.transform.SetParent(this.transform);
                    bubbleObj.transform.localPosition = GameManager.Instance.bubbleOffset;
                    myBubbles.Add(bubbleObj);
                }
            }
        }
    }

    /// <summary>
    /// Hayvanın üzerindeki tüm düşünce balonlarını siler.
    /// </summary>
    public void ClearMyBubbles()
    {
        foreach (Transform child in transform)
        {
            if (child != null && child.GetComponent<ThoughtBubbleController>() != null)
            {
                Destroy(child.gameObject);
            }
        }
        myBubbles.Clear();
    }

    /// <summary>
    /// Bu hayvan objesi yok edilirken balonlarının da silindiğinden emin ol.
    /// </summary>
    void OnDestroy()
    {
        ClearMyBubbles();

        StopIdleAnimationRoutine();
    }

    private void StartIdleAnimationRoutine()
    {
        // Eğer zaten çalışan bir döngü varsa, önce onu durdur (güvenlik önlemi).
        StopIdleAnimationRoutine();
        // Yeni bir Coroutine başlat ve referansını sakla.
        idleAnimationCoroutine = StartCoroutine(IdleAnimationRoutine());
    }

    private void StopIdleAnimationRoutine()
    {
        // Eğer çalışan bir Coroutine varsa, onu durdur.
        if (idleAnimationCoroutine != null)
        {
            StopCoroutine(idleAnimationCoroutine);
            idleAnimationCoroutine = null; // Referansı temizle.
        }
    }

    private IEnumerator IdleAnimationRoutine()
    {
        // Bu döngü, hayvan oturduğu sürece sonsuza dek devam eder.
        while (true)
        {
            // Minimum ve maksimum aralık arasında rastgele bir bekleme süresi hesapla.
            float waitTime = Random.Range(minIdleInterval, maxIdleInterval);

            // Hesaplanan süre kadar bekle.
            yield return new WaitForSeconds(waitTime);

            // Bekleme bittikten sonra, eğer hala oturuyorsak ve animator geçerliyse...
            if (isSeated && animator != null)
            {
                // Animator'deki trigger'ı ateşle.
                animator.SetTrigger(idleAnimationTriggerName);
            }
        }
    }
}
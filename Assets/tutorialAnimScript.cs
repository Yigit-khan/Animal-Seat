using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Rendering;

// Durumları yönetmek için enum
public enum TutorialState
{
    AnimatingIn,
    WritingText,
    WaitingForClose,
    Closing // Yeni durum: Zorla kapatılıyor
}

public class TutorialAnimScript : MonoBehaviour
{
    public static TutorialAnimScript Instance { get; private set; }

    [Header("Panel Animasyonu")]
    public float slideDuration = 0.5f;
    public float slideOffsetY = 1000f;

    [Header("İçerik")]
    public GameObject handImage;
    public TextMeshProUGUI tutorialText;

    [Header("Animasyon Süreleri")]
    public float textAnimationDuration = 2f;

    [Header("Interactable object")]
    public GameObject interactableObject;

    public bool recallTutorial = false;

    [Space]
    [Header("Highlight Animation Settings")]
    [Tooltip("Hightlight sırasında slot'un ölçeğinin çarpanını belirler (1 = orijinal boyut).")]
    [SerializeField] private float highlightScale = 1.2f;
    [Tooltip("Vurgu animasyonunun toplam süresi (saniye).")]
    [SerializeField] private float highlightDuration = 0.5f;
    [Tooltip("Animasyonun kaç titreşimle (vibrato) oynayacağını ayarlar.")]
    [SerializeField] private int highlightVibrato = 10;
    [Tooltip("Punch animasyonunun esneklik parametresi (0–1 arası).")]
    [SerializeField] private float highlightElasticity = 1f;

    // --- Özel Değişkenler ---
    private TutorialState currentState;
    private Vector2 initialPosition;
    private Tween textAnimationTween;
    private string fullText;

    // Animasyon degisgenleri
    [SerializeField] private float singlePulseDuration = 0.5f;
    [SerializeField] private Ease pulseEase = Ease.InOutSine;
    [HideInInspector] public bool isAnimating = false;
    private Vector3 _initialScale;
    private Tween _pulseTween;


    void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        if (Instance != this)
        {
            Instance = this;
        }

        currentState = TutorialState.AnimatingIn;
        if (tutorialText != null)
        {
            fullText = tutorialText.text;
            tutorialText.text = "";
        }
    }

    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.SetAsLastSibling();

        initialPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = initialPosition - new Vector2(0, slideOffsetY);

        if (handImage == null || tutorialText == null)
        {
            Debug.LogWarning("Tutorial animasyonda null GameObject var, animasyon yapilmayacak");
            currentState = TutorialState.Closing;
            return;
        }

        Image handImg = handImage?.GetComponent<Image>();
        if (handImg != null)
        {
            handImg.color = new Color(handImg.color.r, handImg.color.g, handImg.color.b, 0f);
        }

        rectTransform.DOAnchorPos(initialPosition, slideDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                currentState = TutorialState.WritingText;
                StartTextAnimation();
                if (handImg != null) handImg.DOFade(1f, 0.5f);
            });
    }

    void Update()
    {
        // Panel açılırken veya kapanırken tıklamaları yoksay.
        if (currentState == TutorialState.AnimatingIn || currentState == TutorialState.Closing) return;

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            OnPanelClicked();
        }
    }

    private void StartTextAnimation()
    {
        if (tutorialText == null || string.IsNullOrEmpty(fullText))
        {
            currentState = TutorialState.WaitingForClose;
            return;
        }

        int totalChars = fullText.Length;
        textAnimationTween = DOVirtual.Float(0, totalChars, textAnimationDuration, value =>
        {
            int charCount = Mathf.FloorToInt(value);
            tutorialText.text = fullText.Substring(0, charCount);
        })
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            currentState = TutorialState.WaitingForClose;
        });
    }

    public void OnPanelClicked()
    {
        switch (currentState)
        {
            case TutorialState.WritingText:
                if (textAnimationTween != null) 
                    textAnimationTween.Complete();
                Invoke("ClosePanel", 1f);
                break;

            case TutorialState.WaitingForClose:
                ClosePanel();
                break;
        }
    }

    void ClosePanel()
    {
        // Eğer zaten kapanıyorsa tekrar çağırma.
        if (currentState == TutorialState.AnimatingIn || currentState == TutorialState.Closing) return;

      SoundManager.Instance.PlaySFX("UiCloseSound"); 
        currentState = TutorialState.Closing;

        KillAllTweens();

        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 hidePosition = initialPosition - new Vector2(0, slideOffsetY);

        rectTransform.DOAnchorPos(hidePosition, slideDuration)
            .SetEase(Ease.InBack)
            .OnComplete(DestroyContainer);
    }

    // --- YENİ: DIŞARIDAN ÇAĞIRILACAK ACİL KAPATMA FONKSİYONU ---
    public void ForceClose()
    {
        // Eğer zaten kapanıyorsa veya hiç var olmamışsa, bir şey yapma.
        if (this == null || currentState == TutorialState.Closing) return;

        Debug.Log("Tutorial paneli dışarıdan bir komutla kapatılıyor!");
        currentState = TutorialState.Closing;

        KillAllTweens();

        // Animasyonla uğraşmadan anında yok et.
        // Çünkü Win/Lose ekranı daha önemli ve hemen görünmeli.
        DestroyContainer();
    }

    private void KillAllTweens()
    {
        if (textAnimationTween != null && textAnimationTween.IsActive())
        {
            textAnimationTween.Kill();
        }
        // Panelin kendi animasyonunu da öldür (güvenlik önlemi)
        transform.DOKill();
    }

    private void DestroyContainer()
    {
        // Kendini değil, tüm container'ı (blocker dahil) yok et.
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void OnDestroy()
    {
        // Statik referansı temizle ki sahnede hayalet bir referans kalmasın.
        if (Instance == this)
        {
            Instance = null;
        }
        KillAllTweens();
    }
}
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

// Durumları yönetmek için enum
public enum TutorialState
{
    AnimatingIn,
    WritingText,
    WaitingForClose
}

public class tutorialAnimScript : MonoBehaviour
{
    [Header("Panel Animasyonu")]
    public float slideDuration = 0.5f;
    public float slideOffsetY = 1000f;

    [Header("İçerik")]
    public GameObject handImage;
    [Tooltip("Animasyon uygulanacak olan, sahnede hazır bulunan TextMeshPro objesi.")]
    public TextMeshProUGUI tutorialText;

    [Header("Animasyon Süreleri")]
    public float textAnimationDuration = 2f;

    // --- Özel Değişkenler ---
    private TutorialState currentState;
    private Vector2 initialPosition;
    private Tween textAnimationTween;
    private string fullText;

    void Awake()
    {
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

        Image handImg = null;
        if (handImage != null && handImage.TryGetComponent<Image>(out handImg))
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

    // --- DEĞİŞİKLİK BURADA ---
    // Update metodunu, genel tıklama kontrolü için geri getiriyoruz.
    void Update()
    {
        // Panel açılırken veya kapanırken (AnimatingIn durumunda) tıklamaları yoksay.
        if (currentState == TutorialState.AnimatingIn) return;

        // Ekrana tıklandığını veya dokunulduğunu tespit et.
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            // Tıklandığında, panelin kendisine tıklanmış gibi davranmasını sağla.
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

        textAnimationTween = DOVirtual.Float(0, totalChars, textAnimationDuration, (value) =>
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

    // Bu fonksiyon artık doğrudan panelin Button'u tarafından değil, Update içinden çağrılıyor.
    public void OnPanelClicked()
    {
        switch (currentState)
        {
            case TutorialState.WritingText:
                if (textAnimationTween != null) textAnimationTween.Complete();
                break;

            case TutorialState.WaitingForClose:
                ClosePanel();
                break;
        }
    }

    void ClosePanel()
    {
        if (currentState == TutorialState.AnimatingIn) return;
        SoundManager.Instance.PlaySFX("UiCloseSound");
        currentState = TutorialState.AnimatingIn;

        if (textAnimationTween != null && textAnimationTween.IsActive())
        {
            textAnimationTween.Kill();
        }

        if (handImage != null) Destroy(handImage);

        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 hidePosition = initialPosition - new Vector2(0, slideOffsetY);

        rectTransform.DOAnchorPos(hidePosition, slideDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }

    void OnDestroy()
    {
        if (textAnimationTween != null)
        {
            textAnimationTween.Kill();
        }
    }
}
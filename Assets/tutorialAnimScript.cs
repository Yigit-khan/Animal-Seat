using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class tutorialAnimScript : MonoBehaviour
{
    public float slideDuration = 0.5f;
    public float slideOffsetY = 500f;
    public GameObject handImage;

    private Vector2 targetPosition;
    private bool isClosing = false;


    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        // Paneli hierarchy içinde en öne getirme
        rectTransform.SetAsLastSibling();

        // Canvas sıralamasını yükseltme
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            rootCanvas.overrideSorting = true;
            rootCanvas.sortingOrder = 999; // İhtiyacınıza göre değeri ayarlayın
        }

        targetPosition = rectTransform.anchoredPosition;

        // Paneli ekran altına yerleştir
        rectTransform.anchoredPosition = targetPosition - new Vector2(0, slideOffsetY);

        // Yukarı animasyon
        rectTransform.DOAnchorPos(targetPosition, slideDuration).SetEase(Ease.OutBack);

         Image img = handImage.GetComponent<Image>();
    if (img != null)
    {
        Color c = img.color;
        c.a = 0f;
        img.color = c;

       
        img.DOFade(1f, 1f).SetDelay(0.5f);
    }
    }

    void Update()
    {
        if (!isClosing && (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
        {
            ClosePanel();
        }
    }

    void ClosePanel()
    {
        SoundManager.Instance.PlaySFX("UiCloseSound");
        isClosing = true;

        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 hidePosition = targetPosition - new Vector2(0, slideOffsetY);

        Destroy(handImage);

        rectTransform.DOAnchorPos(hidePosition, slideDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }
}

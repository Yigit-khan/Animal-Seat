using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class tutorialAnimScript : MonoBehaviour
{ public float slideDuration = 0.5f;
    public float slideOffsetY = 500f;

    private Vector2 targetPosition;
    private bool isClosing = false;

    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        targetPosition = rectTransform.anchoredPosition;

        // Paneli ekran altına yerleştir
        rectTransform.anchoredPosition = targetPosition - new Vector2(0, slideOffsetY);

        // Yukarı animasyon
        rectTransform.DOAnchorPos(targetPosition, slideDuration).SetEase(Ease.OutBack);
    }

    void Update()
    {
        // Eğer kullanıcı ekrana dokunduysa veya tıkladıysa
        if (!isClosing && (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
        {
            ClosePanel();
        }
    }

    void ClosePanel()
    {
        isClosing = true;

        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 hidePosition = targetPosition - new Vector2(0, slideOffsetY);

        rectTransform.DOAnchorPos(hidePosition, slideDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }

}

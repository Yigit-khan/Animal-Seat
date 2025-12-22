using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ClockBounce : MonoBehaviour
{
    public RectTransform clockRect;

    void Start()
    {
        // Sonsuz yukarý-aþaðý zýplama
        clockRect.DOAnchorPosY(clockRect.anchoredPosition.y + 20f, 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}

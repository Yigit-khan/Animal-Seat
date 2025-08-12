using UnityEngine;
using System.Collections;

public class TutorialAnimation : MonoBehaviour
{
    public RectTransform handImage;
    public RectTransform startTarget;
    public RectTransform endTarget;
    public float moveDuration = 1f;
    public int loopCount = -1; // Sonsuz tekrar için -1

    private void Awake()
    {
        if (handImage != null && startTarget != null)
            handImage.position = startTarget.position;
    }

    private void OnEnable()
    {
        // Eğer referanslardan biri yoksa animasyonu hiç başlatma
        if (handImage != null && startTarget != null && endTarget != null)
            StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        if (handImage == null || startTarget == null || endTarget == null)
            yield break;

        handImage.position = startTarget.position;
        int loops = 0;

        while (loopCount < 0 || loops < loopCount)
        {
            if (handImage == null || startTarget == null || endTarget == null)
                yield break;

            // Başlangıç pozisyonuna git
            handImage.anchoredPosition = startTarget.anchoredPosition;

            // Bitiş pozisyonuna animasyon
            float t = 0;
            while (t < moveDuration)
            {
                if (handImage == null || startTarget == null || endTarget == null)
                    yield break;

                t += Time.deltaTime;
                handImage.anchoredPosition = Vector2.Lerp(
                    startTarget.anchoredPosition,
                    endTarget.anchoredPosition,
                    t / moveDuration
                );
                yield return null;
            }

            yield return new WaitForSeconds(0.3f); // Bekleme
            loops++;
        }
    }
}

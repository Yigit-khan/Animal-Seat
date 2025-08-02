using DG.Tweening;
using UnityEngine;
using System.Collections;
public class TutorialAnimation : MonoBehaviour
{
    public RectTransform handImage;
    public RectTransform startTarget;
    public RectTransform endTarget;
    public float moveDuration = 1f;
    public int loopCount = -1; // Sonsuz tekrar için -1

    private void OnEnable()
    {
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        int loops = 0;

        while (loopCount < 0 || loops < loopCount)
        {
            // Baþlangýç pozisyonuna git
            handImage.anchoredPosition = startTarget.anchoredPosition;

            // Bitiþ pozisyonuna animasyon
            float t = 0;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                handImage.anchoredPosition = Vector2.Lerp(startTarget.anchoredPosition, endTarget.anchoredPosition, t / moveDuration);
                yield return null;
            }

            yield return new WaitForSeconds(0.3f); // Bekleme
            loops++;
        }
    }
}

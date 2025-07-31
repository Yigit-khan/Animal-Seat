using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class PanelWinUIAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] elementsToFadeIn; // sýrayla görünmesini istediðin þeyler
    [SerializeField] private float elementDelay = 0.2f;

    private RectTransform panel;
    private CanvasGroup canvasGroup;
    private Vector2 originalPos;

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPos = panel.anchoredPosition;
    }

    public void Show()
    {
        panel.localScale = Vector3.zero;
        panel.anchoredPosition = originalPos + new Vector2(0, 300);
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(panel.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
        seq.Join(panel.DOAnchorPos(originalPos, 0.4f).SetEase(Ease.OutCubic));
        seq.Join(canvasGroup.DOFade(1f, 0.4f));
        seq.OnComplete(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            StartCoroutine(FadeInElementsSequentially());
        });
    }

    private IEnumerator FadeInElementsSequentially()
    {
        foreach (CanvasGroup cg in elementsToFadeIn)
        {
            cg.alpha = 0;
            cg.gameObject.SetActive(true);
            cg.DOFade(1f, 0.3f);
            yield return new WaitForSeconds(elementDelay);
        }
    }
}
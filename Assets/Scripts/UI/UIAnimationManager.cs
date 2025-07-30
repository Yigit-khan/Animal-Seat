using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIAnimationManager : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private RectTransform coinSpawnOrigin; // Coin’lerin çýkýþ noktasý
    [SerializeField] private RectTransform coinTarget; // Rewards/icon objesi
    [SerializeField] private TMP_Text coinText; // Altýn sayýsýný gösteren text
    [SerializeField] private int coinsToCollect = 20;
    [SerializeField] private int currentCoins = 1000;

    public void OnEnable()
    {
        CollectCoins();
    }
    public void CollectCoins(bool x2 = false)
    {
        float spawnRadius = 60f;
        int collectedCoins = 0;
        int animCount = x2 ? coinsToCollect * 2 : coinsToCollect;
        int coinCount = currentCoins;
        int decrement = (animCount > 0) ? Mathf.CeilToInt((float)currentCoins / animCount) : 1;

        float spawnInterval = 0.03f;
        float moveTime = 1f;
        float delayMultiplier = 0.2f;

        coinText.text = currentCoins.ToString("N0");

        for (int i = 0; i < animCount; i++)
        {
            GameObject coin = Instantiate(coinPrefab, coinSpawnOrigin.parent);
            float angle = i * (360f / 10);
            float radians = angle * Mathf.Deg2Rad;

            Vector3 spawnOffset = new Vector3(
                Mathf.Cos(radians) * spawnRadius,
                Mathf.Sin(radians) * spawnRadius,
                0
            );
            coin.transform.position = coinSpawnOrigin.position + spawnOffset;

            Vector3 targetPosition = coinTarget.position;
            float delay = i * delayMultiplier;

            coin.transform.DOMove(targetPosition, moveTime)
                .SetDelay(delay / 5).SetEase(Ease.InOutBack)
                .OnComplete(() =>
                {
                    Destroy(coin);
                    coinCount -= decrement;
                    if (coinCount < 0) coinCount = 0;
                    collectedCoins++;

                    coinText.text = coinCount.ToString("N0");

                    if (collectedCoins >= animCount)
                    {
                        Debug.Log("Coin animation complete");
                    }
                });
        }
    }
}


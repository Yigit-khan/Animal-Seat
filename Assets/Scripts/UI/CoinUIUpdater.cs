using UnityEngine;
using TMPro;

// Bu script, bir TMP_Text component'inin olmasýný zorunlu kýlar.
[RequireComponent(typeof(TMP_Text))]
public class CoinUIUpdater : MonoBehaviour
{
    private TMP_Text coinText;

    private void Awake()
    {
        // Kendi üzerindeki TMP_Text component'ini bul ve sakla.
        coinText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        // CoinManager'daki event'e abone ol (dinlemeye baþla).
        CoinManager.OnCoinsChanged += UpdateCoinText;

        // Abone olduktan sonra mevcut deðeri de hemen göster.
        if (CoinManager.Instance != null)
        {
            UpdateCoinText(CoinManager.Instance.CurrentCoins);
        }
    }

    private void OnDisable()
    {
        // Obje kapandýðýnda veya yok olduðunda abonelikten çýk.
        // Bu, hatalarý önlemek için ÇOK önemlidir.
        CoinManager.OnCoinsChanged -= UpdateCoinText;
    }

    /// <summary>
    /// CoinManager event'i tarafýndan çaðrýlan fonksiyon.
    /// </summary>
    private void UpdateCoinText(int newAmount)
    {
        if (coinText != null)
        {
            coinText.text = newAmount.ToString();
        }
    }
}
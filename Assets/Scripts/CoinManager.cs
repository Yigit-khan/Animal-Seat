using System;
using UnityEngine;

/// <summary>
/// Oyuncunun coin verisini yöneten ve oyun oturumları arasında saklayan Singleton sistemi.
/// Bu sistem, sahneye boş bir GameObject üzerine eklenerek kullanılır.
/// </summary>
public class CoinManager : MonoBehaviour
{
    // --- Singleton Deseni ---
    // Bu, oyunun herhangi bir yerinden CoinManager.Instance diyerek bu scripte erişmemizi sağlar.
    public static CoinManager Instance { get; private set; }

    /// <summary>
    /// Oyuncunun mevcut coin miktarı. Dışarıdan sadece okunabilir.
    /// </summary>
    public int CurrentCoins { get; private set; }

    public static event Action<int> OnCoinsChanged;

    // Coin verisini PlayerPrefs'te saklamak için kullanılacak anahtar.
    private const string COINS_SAVE_KEY = "PlayerTotalCoins";

    private void Awake()
    {
        // Singleton desenini kur: Sadece bir tane CoinManager olmasına izin ver.
        if (Instance != null && Instance != this)
        {
            // Eğer sahnede zaten bir CoinManager varsa, bu yenisini yok et.
            // Destroy(this.gameObject);
        }
        else
        {
            // Bu, ilk ve tek CoinManager.
            Instance = this;
            // Bu objenin, yeni sahneler yüklendiğinde yok olmamasını sağla.
            DontDestroyOnLoad(this.gameObject);

            // Oyun başlarken kayıtlı coin'leri yükle.
            LoadCoins();
        }
    }

    /// <summary>
    /// Oyuncunun coin miktarını artırır ve yeni değeri kaydeder.
    /// </summary>
    /// <param name="amount">Eklenecek coin miktarı.</param>
    /*
    public void AddCoins(int amount)
    {
        if (amount < 0) return; 

        CurrentCoins += amount;
        SaveCoins();
    }
    */
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        CurrentCoins += amount;
        SaveCoins();
        // Event'i tetikle!
        OnCoinsChanged?.Invoke(CurrentCoins);
    }

    /// <summary>
    /// Oyuncunun coin'ini harcamaya çalışır.
    /// </summary>
    /// <param name="amount">Harcanacak miktar.</param>
    /// <returns>Harcama başarılıysa true, bakiye yetersizse false döner.</returns>
    public bool SpendCoins(int amount)
    {
        if (amount < 0) return false;

        if (CurrentCoins >= amount)
        {
            CurrentCoins -= amount;
            SaveCoins();
            return true;
        }

        // Bakiye yetersiz.
        return false;
    }

    /// <summary>
    /// Coin verisini PlayerPrefs kullanarak cihaz hafızasına kaydeder.
    /// </summary>
    private void SaveCoins()
    {
        Debug.Log("Current coins: " + CurrentCoins);
        PlayerPrefs.SetInt(COINS_SAVE_KEY, CurrentCoins);
        PlayerPrefs.Save(); // Değişikliklerin diske hemen yazılmasını garantiler.
    }

    /// <summary>
    /// Coin verisini cihaz hafızasından yükler. Kayıt yoksa 0'dan başlar.
    /// </summary>
    private void LoadCoins()
    {
        CurrentCoins = PlayerPrefs.GetInt("PlayerTotalCoins", 0);
        OnCoinsChanged?.Invoke(CurrentCoins);

    }
}
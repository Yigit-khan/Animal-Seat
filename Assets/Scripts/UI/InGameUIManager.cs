// InGameUIManager.cs
using TMPro;
using UnityEngine;

public class InGameUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Kazanma ekraný olan UI panelini (WinUI) buraya sürükleyin.")]
    [SerializeField] private GameObject winUI;

    [Tooltip("Kaybetme ekraný olan UI panelini (LoseUI) buraya sürükleyin.")]
    [SerializeField] private GameObject loseUI; // YENÝ EKLENDÝ

    [Header("Game Canvases")]
    [Tooltip("Kapatýlacak olan oyun içi ana canvas'ý (GameCanvasSystem) buraya sürükleyin.")]
    [SerializeField] private GameObject gameCanvas;

    // --- WinUIAnimation fonksiyonu olduðu gibi kalýyor ---


   
    public void WinUIAnimation()
    {
        // ... (deðiþiklik yok)
        if (gameCanvas != null) gameCanvas.SetActive(false);
        if (winUI != null)
        {
            winUI.SetActive(true);
            PanelWinUIAnimator animator = winUI.GetComponent<PanelWinUIAnimator>();
            if (animator != null) animator.Show();
            else Debug.LogError("WinUI objesinde 'PanelWinUIAnimator' script'i bulunamadý!");
        }
        else Debug.LogError("InGameUIManager'da 'Win UI' referansý atanmamýþ!");
    }

    // --- YENÝ FONKSÝYON 
    public void ShowLoseUI(string loseReason)
    {
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(false);
        }

        if (loseUI != null)
        {
            // LoseUI panelini aktifleþtir.
            loseUI.SetActive(true);

            // Panel üzerindeki UIAnimationManager'ý bul.
            UIAnimationManager losePanelController = loseUI.GetComponent<UIAnimationManager>();
            if (losePanelController != null)
            {
                // Metni ayarlamasý için ona komut gönder.
                losePanelController.SetupLoseScreen(loseReason); // YENÝ: Metni paslýyoruz.
            }

            PanelWinUIAnimator animator = loseUI.GetComponent<PanelWinUIAnimator>();
            if (animator != null) animator.Show();
            else Debug.LogError("LoseUI objesinde 'PanelWinUIAnimator' script'i bulunamadý!");
        }
        else
        {
            Debug.LogError("InGameUIManager'da 'Lose UI' referansý atanmamýþ!");
        }
    }

}
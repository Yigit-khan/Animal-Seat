using UnityEngine;

public class InGameUIManager : MonoBehaviour
{
 
    /*
    public void WinUIAnimation()
    {
        
        winUI.SetActive(true);
        // Paneli aktif edip animasyonla göster
        winUI.GetComponent<PanelWinUIAnimator>().Show();

    }
    */
    [Tooltip("Kazanma ekraný olan UI panelini (WinUI) buraya sürükleyin.")]
    [SerializeField] private GameObject winUI;

    [Tooltip("Kapatýlacak olan oyun içi ana canvas'ý (GameCanvasSystem) buraya sürükleyin.")]
    [SerializeField] private GameObject gameCanvas; // YENÝ EKLENDÝ

    public void WinUIAnimation()
    {
        // 1. Oyun içi ana canvas'ý kapat
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(false);
        }
        else
        {
            Debug.LogWarning("InGameUIManager'da 'Game Canvas' referansý atanmamýþ. Kapatýlacak bir canvas bulunamadý.");
        }

        // 2. Kazanma ekranýný (WinUI) göster
        if (winUI != null)
        {
            winUI.SetActive(true);

            // Panelin üzerindeki animatör script'ini alýp animasyonu baþlat
            PanelWinUIAnimator animator = winUI.GetComponent<PanelWinUIAnimator>();
            if (animator != null)
            {
                animator.Show();
            }
            else
            {
                Debug.LogError("WinUI objesinde 'PanelWinUIAnimator' script'i bulunamadý!");
            }
        }
        else
        {
            Debug.LogError("InGameUIManager'da 'Win UI' referansý atanmamýþ! Kazanma ekraný gösterilemiyor.");
        }
    }
}

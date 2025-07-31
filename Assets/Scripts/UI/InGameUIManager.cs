using UnityEngine;

public class InGameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject winUI;
    public void WinUIAnimation()
    {
        
        winUI.SetActive(true);
        // Paneli aktif edip animasyonla göster
        winUI.GetComponent<PanelWinUIAnimator>().Show();

    }
}

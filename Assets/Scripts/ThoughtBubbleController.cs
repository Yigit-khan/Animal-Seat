using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Artýk EventSystems arayüzüne ihtiyacýmýz yok.
// using UnityEngine.EventSystems;

public class ThoughtBubbleController : MonoBehaviour
{
    [Header("Bileþen Referanslarý")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void Initialize(Sprite icon, string description)
    {
        if (iconImage != null)
            iconImage.sprite = icon;

        if (descriptionText != null)
            descriptionText.text = description;

        if (descriptionPanel != null)
            descriptionPanel.SetActive(false);
    }

    // --- YENÝ FONKSÝYON ---
    // Bu fonksiyonu public yaparak Button'ýn OnClick olayýna baðlayacaðýz.
    // Artýk IPointerClickHandler'dan gelen OnPointerClick metoduna ihtiyacýmýz yok.
    public void ToggleDescriptionPanel()
    {
        if (descriptionPanel != null)
        {
            // Panelin mevcut durumunun tersini ayarla (açýksa kapat, kapalýysa aç).
            descriptionPanel.SetActive(!descriptionPanel.activeSelf);
        }
    }
}
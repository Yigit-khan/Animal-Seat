using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThoughtBubbleController : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TextMeshProUGUI descriptionText;

    // Artýk targetToFollow ve offset deðiþkenlerine ihtiyacýmýz yok.
    // Update fonksiyonuna da ihtiyacýmýz yok.

    // Initialize metodu da basitleþiyor.
    public void Initialize(Sprite icon, string description)
    {
        iconImage.sprite = icon;
        descriptionText.text = description;

        descriptionPanel.SetActive(false);
    }

    // Bu fonksiyon ayný kalýr.
    public void OnBubbleClicked()
    {
        descriptionPanel.SetActive(!descriptionPanel.activeSelf);
    }

    // Hayvan yok olduðunda balonun da yok olmasýný saðlamak için
    // parent'ý kaybolursa kendini yok etmesi faydalý olabilir.
    void Update()
    {
        // Eðer bir parent'ý yoksa (hayvan silinmiþse), kendini yok et.
        if (transform.parent == null)
        {
            Destroy(gameObject);
        }
    }
}
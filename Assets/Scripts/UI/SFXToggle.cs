using UnityEngine;
using UnityEngine.UI;

public class SFXToggle : MonoBehaviour
{
    public Image iconImage;          // Ýkonu gösterecek Image
    public Image backgroundImage;    // Arka plan Image

    public Sprite onSprite;          // Tik ikonu
    public Sprite offSprite;         // Çarpý ikonu

    public Color onColor = Color.green;
    public Color offColor = Color.red;

    private bool isOn = true;


    private void Start()
    {
        if (SoundManager.Instance != null)
            isOn = SoundManager.Instance.sfxEnabled;

        UpdateVisual();
    }


    public void ToggleSFX()
    {
        if (SoundManager.Instance != null)
        {
            isOn = !SoundManager.Instance.sfxEnabled;
            SoundManager.Instance.sfxEnabled = isOn;
            SoundManager.Instance.ApplySFXState();
        }

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // Arka plan rengini deðiþtirir
        backgroundImage.color = isOn ? onColor : offColor;
        // Ýkona tik veya çarpý atar
        iconImage.sprite = isOn ? onSprite : offSprite;
      

        // Ýkonu aktif/pasif yapma, her zaman göster (sadece sprite deðiþir)
        iconImage.enabled = true;
    }
}

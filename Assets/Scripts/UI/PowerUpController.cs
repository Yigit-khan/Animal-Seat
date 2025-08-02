using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

[System.Serializable]
public class PowerUpUIReference
{
    [Tooltip("ScriptableObject verisi")]
    public PowerupSO so;

    [Tooltip("Sahnedeki Button bileşeni")]
    public string uiButtonObjectName;

    [Tooltip("Sahnedeki TextMeshProUGUI bileşeni")]
    public string uiTextObjectName;

    [HideInInspector] public Button uiButton;

    [HideInInspector] public TextMeshProUGUI uiText;
}

public class PowerUpController : MonoBehaviour
{
    public static PowerUpController Instance;

    [Tooltip("Her power-up için SO, Button ve Text referanslarını ekleyin")]
    [SerializeField] private List<PowerUpUIReference> powerUps;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (var entry in powerUps)
        {
            // Button ataması
            GameObject btnGO = GameObject.Find(entry.uiButtonObjectName);
            if (btnGO != null)
            {
                entry.uiButton = btnGO.GetComponent<Button>();
                if (entry.uiButton == null)
                    Debug.LogError($"GameObject '{entry.uiButtonObjectName}' üzerinde Button component'i yok!");
            }
            else
            {
                Debug.LogError($"Button GameObject '{entry.uiButtonObjectName}' bulunamadı!");
            }

            // Text ataması
            GameObject txtGO = GameObject.Find(entry.uiTextObjectName);
            if (txtGO != null)
            {
                entry.uiText = txtGO.GetComponent<TextMeshProUGUI>();
                if (entry.uiText == null)
                    Debug.LogError($"GameObject '{entry.uiTextObjectName}' üzerinde TextMeshProUGUI component'i yok!");
            }
            else
            {
                Debug.LogError($"Text GameObject '{entry.uiTextObjectName}' bulunamadı!");
            }
        }

        for (int i = 0; i < powerUps.Count; i++)
        {
            var original = powerUps[i].so;
            var clone = ScriptableObject.Instantiate(original);
            powerUps[i].so = clone;
        }
    }

    private void Start()
    {
        foreach (var entry in powerUps)
        {
            // 1) Başlangıç ölçeğini SO içine kaydet
            entry.so.recallButtonInitialScale = entry.uiButton.transform.localScale;

            // 2) Butona tıklama event’i ekle
            entry.uiButton.onClick.AddListener(() => OnPowerUpClick(entry));

            // 3) UI’ı başlangıç değeriyle güncelle
            entry.uiText.text = entry.so.remainingUse.ToString();
        }
    }

    private void OnPowerUpClick(PowerUpUIReference entry)
    {
        var so = entry.so;
        if (so.remainingUse <= 0)
            return;

        // b) Doğru power-up modunu başlat
        switch (so.powerupName)
        {
            case "Recall":
                GameManager.Instance.ActivateRecallMode();
                break;
            case "Eyepatch":
                GameManager.Instance.ActivateEyepatchMode();
                break;
                // Yeni power-up’lar eklenecekse case bloklarına ekleyin
        }

        // c) Ses efekti çal (powerupName ile eşleşen klip adına göre)
        SoundManager.Instance.PlaySFX(so.powerupName);
    }

    /// <summary>
    /// Dışarıdan çağrılarak belirli bir power-up’ın buton görselini aktif/pasif yapar.
    /// </summary>
    public void SetPowerUpVisuals(PowerupSO so, bool isActive)
    {
        var entry = powerUps.Find(e => e.so == so);
        if (entry == null) return;

        // Mevcut animasyonları sonlandır
        entry.uiButton.transform.DOKill();

        // Hedef ölçeği hesapla
        Vector3 initialScale = so.recallButtonInitialScale;
        Vector3 targetScale = isActive
            ? initialScale * so.buttonScaleAmount
            : initialScale;

        // Animasyonu uygula
        entry.uiButton.transform
            .DOScale(targetScale, so.buttonAnimationDuration)
            .SetEase(Ease.OutBack);
    }

    /// <summary>
    /// powerupName ile eşleşen UI referansını bulur.
    /// </summary>
    public PowerUpUIReference GetReferenceByName(string powerupName)
    {
        return powerUps.Find(e =>
            e.so.powerupName.Equals(powerupName, StringComparison.OrdinalIgnoreCase)
        );
    }

    public void DecreaseRemainingUse(PowerupSO powerup)
    {
        PowerUpUIReference reference = GetReferenceByName(powerup.powerupName);

        // a) Kalan hakkı düşür ve UI’ı güncelle
        powerup.remainingUse--;
        reference.uiText.text = powerup.remainingUse.ToString();
    }

}

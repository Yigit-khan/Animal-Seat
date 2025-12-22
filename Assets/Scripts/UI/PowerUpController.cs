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
                    Debug.LogWarning($"GameObject '{entry.uiButtonObjectName}' üzerinde Button component'i yok!");
            }
            else
            {
                Debug.LogWarning($"Button GameObject '{entry.uiButtonObjectName}' bulunamadı!");
            }

            // Text ataması
            GameObject txtGO = GameObject.Find(entry.uiTextObjectName);
            if (txtGO != null)
            {
                entry.uiText = txtGO.GetComponent<TextMeshProUGUI>();
                if (entry.uiText == null)
                    Debug.LogWarning($"GameObject '{entry.uiTextObjectName}' üzerinde TextMeshProUGUI component'i yok!");
            }
            else
            {
                Debug.LogWarning($"Text GameObject '{entry.uiTextObjectName}' bulunamadı!");
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
            if (entry.uiButton == null || entry.so == null)
                continue;

            Debug.Log("EKLENIYOR: " + entry.so.name);
            // 1) Başlangıç ölçeğini SO içine kaydet
            entry.so.buttonInitialScale = entry.uiButton.transform.localScale;

            // 2) Butona tıklama event’i ekle
            entry.uiButton.onClick.AddListener(() => OnPowerUpClick(entry));

            // 3) UI’ı başlangıç değeriyle güncelle
            entry.uiText.text = entry.so.RemainingUse.ToString();

            if (entry.uiButton.gameObject.tag == "Tutorial")
            {
                GameManager.Instance.StartPulsingObject(entry.uiButton.gameObject);
                entry.so.isPulsing = true;

                if (entry.so.powerupName == "Recall")
                {
                    entry.uiButton.gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnEnable()
    {
        GameManager.OnFirstAnimalPlaced += ShowRecallButton;
    }

    private void ShowRecallButton()
    {
        PowerUpUIReference recallRef = GetReferenceByName("Recall");

        Debug.Log(
            $"recallRef != null: {(recallRef != null)}, " +
            $"recallRef.so.isPulsing: {recallRef.so.isPulsing}"
        );

        if (recallRef != null && recallRef.so.isPulsing)
        {
            recallRef.uiButton.gameObject.SetActive(true);
            GameManager.Instance.tutorialInteractableObject = recallRef.uiButton.gameObject;
            Debug.Log("OnFirstAnimalPlaced anonsu alındı! Recall butonu şimdi görünür.");

            if (TutorialAnimScript.Instance != null)
            {
                TutorialAnimScript.Instance.AnimatePanel();
            }
            else
            {
                Debug.LogWarning("TutorialAnimScript null oldugu icin getirilemedi.");
            }
        }
    }

    private void OnPowerUpClick(PowerUpUIReference entry)
    {
        var so = entry.so;
        if (so.RemainingUse <= 0)
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
        Vector3 initialScale = so.buttonInitialScale;
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
        powerup.RemainingUse--;
        reference.uiText.text = powerup.RemainingUse.ToString();
    }

    public PowerUpUIReference GetPowerUpUIReferenceBySO(PowerupSO powerupSO)
    {
        // Metoda geçersiz bir referans verilip verilmediğini kontrol et
        if (powerupSO == null)
        {
            return null;
        }

        // List.Find metodu ile, listedeki her bir 'e' elemanının 'so' alanının,
        // metoda verilen 'powerupSO' ile aynı olup olmadığını kontrol et.
        return powerUps.Find(e => e.so == powerupSO);
    }

}

using UnityEngine;
using TMPro;              // ← Bunu ekleyin
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PowerupSO_", menuName = "Scriptable Objects/PowerUp")]
public class PowerupSO : ScriptableObject
{
    #region AYARLAR

    [Header("Ayarlar")]
    [Tooltip("Powerup'ın ismi.")]
    public string powerupName;
    [Tooltip("Oyuncunun her seviye başında sahip olacağı geri alma hakkı sayısı.")]
    public int remainingUse;

    #endregion

    #region ANIMASYON

    [Header("Animasyon Ayarları")]
    [Tooltip("Butonun büyüme oranı (örneğin 1.2 = %120).")]
    public float buttonScaleAmount = 1.2f;
    [Tooltip("Buton animasyon süresi (saniye).")]
    public float buttonAnimationDuration = 0.3f;
    [HideInInspector] public Vector3 recallButtonInitialScale;

    #endregion

    #region SES

    [Header("Ses ayarları")]
    [Tooltip("Ses dosyası ismi")]
    public string soundFileName;

    #endregion
}

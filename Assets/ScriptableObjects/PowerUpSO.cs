using UnityEngine;
using TMPro;              // ← Bunu ekleyin
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PowerupSO_", menuName = "Scriptable Objects/PowerUp")]
public class PowerupSO : ScriptableObject
{
        #region AYARLAR

        [Header("Ayarlar")]
        public string powerupName;
        [SerializeField] private int defaultUses = 2;

        private int _cachedUses;
         public int RemainingUse
        {
            get
            {
                // defaultUses kullanarak, recursion’dan kaçınıyoruz
                return PlayerPrefs.GetInt(powerupName, defaultUses);
            }
            set
            {
                PlayerPrefs.SetInt(powerupName, value);
                // Sık sık I/O istemiyorsanız, buradaki Save() çağrısını oyunun kapanışı sırasında veya kontrol ettiğiniz başka bir noktada tek seferlik yapabilirsiniz.
                PlayerPrefs.Save();
            }
        }

    // Oyunun başında veya ihtiyacınız olduğunda çağırın



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

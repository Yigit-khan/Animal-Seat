using UnityEngine;
using DG.Tweening;  // DOTween

public enum SlotState { Unlocked, Locked, Occupied }

public class HoldingSlotController : MonoBehaviour
{
    [Header("Slot Settings")]
    public SlotState CurrentState { get; private set; }
    public AnimalController OccupyingAnimal { get; private set; }

    [Space]
    [Header("Highlight Animation Settings")]
    [Tooltip("Hightlight sırasında slot'un ölçeğinin çarpanını belirler (1 = orijinal boyut).")]
    [SerializeField] private float highlightScale = 1.2f;
    [Tooltip("Vurgu animasyonunun toplam süresi (saniye).")]
    [SerializeField] private float highlightDuration = 0.5f;
    [Tooltip("Animasyonun kaç titreşimle (vibrato) oynayacağını ayarlar.")]
    [SerializeField] private int highlightVibrato = 10;
    [Tooltip("Punch animasyonunun esneklik parametresi (0–1 arası).")]
    [SerializeField] private float highlightElasticity = 1f;

    [SerializeField] private float singlePulseDuration = 0.5f;
    [SerializeField] private Ease pulseEase = Ease.InOutSine;
    private Vector3 _initialScale;
    private Tween _pulseTween;
    private bool isAnimating = false;

    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    private Material highlightMaterial;


    private void Awake()
    {
        _initialScale = transform.localScale;
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }
        highlightMaterial = GameManager.Instance.greenEffectAreaMaterial;
    }

    public void Initialize(SlotState initialState)
    {
        CurrentState = initialState;
    }

    public void SetState(SlotState newState)
    {
        CurrentState = newState;
    }

    public void PlaceAnimal(AnimalController animal)
    {
        if (CurrentState == SlotState.Unlocked)
        {
            if (isAnimating)
                ClearAnim();
            OccupyingAnimal = animal;
            SetState(SlotState.Occupied);
        }
    }

    public PickUpResult PickUpAnimal()
    {
        if (CurrentState == SlotState.Occupied)
        {
            AnimalController animalToReturn = OccupyingAnimal;
            OccupyingAnimal = null;
            SetState(SlotState.Unlocked);
            return new PickUpResult(animalToReturn, true);
        }
        return new PickUpResult(null, false);
    }

    public void Highlight(Material highlightMaterial)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material = highlightMaterial;
        }
    }
    public void ResetHighlight()
    {
        if (meshRenderer != null && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }
    }

    /// <summary>
    /// Slot'u sürekli vurgu (pulsate) animasyonuyla gösterir.
    /// Reset edilmediği sürece döngü devam eder.
    /// </summary>
    public void TutorialScaleAnim()
    {
        // Eğer zaten bir pulsatör tween varsa üzerine yeni kurma
        if (_pulseTween != null && _pulseTween.IsActive()) return;

        isAnimating = true;
        // İlk önce varsa önceki tüm animasyonları durdur
        transform.DOKill();

        // Ölçeği büyütüp küçülten sonsuz döngü
        _pulseTween = transform
            .DOScale(_initialScale * highlightScale, singlePulseDuration)
            .SetEase(pulseEase)
            .SetLoops(-1, LoopType.Yoyo)
            .SetId(this); // tween'i bu obje ile ilişkilendir
    }

    /// <summary>
    /// Highlight'ı durdurur ve ölçeği orijinal haline çeker.
    /// </summary>
    public void ClearAnim()
    {
        isAnimating = false;

        // Döngüsel tween'i durdur
        if (_pulseTween != null)
        {
            _pulseTween.Kill();
            _pulseTween = null;
        }

        // Tüm diğer DOTween animasyonlarını iptal et
        transform.DOKill();

        // Orijinal ölçeğe hızlıca dönelim
        transform
            .DOScale(_initialScale, singlePulseDuration * 0.5f)
            .SetEase(Ease.OutSine);
    }
}

// Yeni bir yardýmcý struct, hayvaný ve iþlemin baþarýlý olup olmadýðýný döndürmek için.
public struct PickUpResult
{
    public AnimalController Animal;
    public bool Success;

    public PickUpResult(AnimalController animal, bool success)
    {
        Animal = animal;
        Success = success;
    }
}
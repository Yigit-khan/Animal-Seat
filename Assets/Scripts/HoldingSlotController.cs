using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;  // DOTween

public enum SlotState {Unlocked, Locked, Occupied}

public class HoldingSlotController : MonoBehaviour
{
    [Header("Slot Settings")]
    public SlotState CurrentState { get; private set; }
    public AnimalController OccupyingAnimal { get; private set; }
    public bool isPulsing = false;
    public Vector3 originalScale;

    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    private Material highlightMaterial;

    

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }
        highlightMaterial = GameManager.Instance.greenEffectAreaMaterial;
        originalScale = transform.localScale;
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
        if (CurrentState != SlotState.Unlocked) return;
        
        if (isPulsing)
        {
            GameManager.Instance.StopPulsingObject(transform.gameObject, originalScale);
            isPulsing = false;
        }

        OccupyingAnimal = animal;
        SetState(SlotState.Occupied);
    }

    public PickUpResult PickUpAnimal()
    {
        if (CurrentState != SlotState.Occupied)
            return new PickUpResult(null, false);
        
        AnimalController animalToReturn = OccupyingAnimal;
        OccupyingAnimal = null;
        SetState(SlotState.Unlocked);
        return new PickUpResult(animalToReturn, true);
        
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
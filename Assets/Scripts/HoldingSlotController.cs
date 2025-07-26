using UnityEngine;

public enum SlotState { Unlocked, Locked, Occupied }

public class HoldingSlotController : MonoBehaviour
{
    // Bu deðiþkenlere hala ihtiyacýmýz var.
    public SlotState CurrentState { get; private set; }
    public AnimalController OccupyingAnimal { get; private set; }

    // LockVisual'a artýk gerek yok.
    // [SerializeField] private GameObject lockVisual; 

    public void Initialize(SlotState initialState)
    {
        CurrentState = initialState;
    }

    // Artýk state'i dýþarýdan deðiþtireceðiz, bu yüzden bu metodu public yapýyoruz.
    public void SetState(SlotState newState)
    {
        CurrentState = newState;
    }

    public void PlaceAnimal(AnimalController animal)
    {
        if (CurrentState == SlotState.Unlocked)
        {
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
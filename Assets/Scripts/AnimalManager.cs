using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class AnimalManager
{
    private List<AnimalSO> _animalDatas;


    [ContextMenu("Check All Interactions Now")]

    public bool IsAllInteractionsValid(List<AnimalSO> allAnimals, out List<AnimalSO> affectedAnimals)
    {
        affectedAnimals = new List<AnimalSO>();
        if (allAnimals == null) return true;

        var seatedAnimals = allAnimals.Where(a => a != null && a.gridOriginPos.x >= 0).ToList();

        foreach (var ownerAnimal in seatedAnimals) // Kuralın sahibi
        {
            foreach (var otherAnimal in seatedAnimals) // Etkilenebilecek komşu
            {
                if (ownerAnimal == otherAnimal) continue;

                // ownerAnimal'ın trait'leri otherAnimal'ı rahatsız ediyor mu?
                CheckForVictims(ownerAnimal, otherAnimal, affectedAnimals);
            }
        }

        return affectedAnimals.Count == 0;
    }

    private void CheckForVictims(AnimalSO owner, AnimalSO target, List<AnimalSO> victims)
    {
        foreach (var trait in owner.traits)
        {
            var affectedPoints = trait.GetTraitEffectPoints(owner.gridOriginPos, owner.size);

            if (affectedPoints.Any(p => p == target.gridOriginPos))
            {
                bool isAnti = trait.antiToEveryTrait || trait.antiTraits.Any(antiTrait => target.traits.Contains(antiTrait));

                if (isAnti)
                {
                    // Kural ihlal edildi. "Etkilenen" (kurban) olan 'target' hayvanıdır.
                    if (!victims.Contains(target))
                    {
                        victims.Add(target);
                    }
                }
            }
        }
    }

    public bool IsAllInteractionsValid(List<AnimalSO> animalDatas)
    {
        return IsAllInteractionsValid(animalDatas, out _);
    }

    public bool IsSoftLocked(List<AnimalSO> waitingAnimals, List<SeatController> emptySeats, List<AnimalSO> seatedAnimals)
    {
        if (emptySeats == null || emptySeats.Count == 0 && waitingAnimals.Count > 0) return true;

        foreach (var animalToTest in waitingAnimals)
        {
            foreach (var seatToTest in emptySeats)
            {
                var hypotheticalBoardState = new List<AnimalSO>(seatedAnimals) { animalToTest };
                Vector2Int originalPos = animalToTest.gridOriginPos;
                animalToTest.gridOriginPos = seatToTest.GridPosition;

                if (IsAllInteractionsValid(hypotheticalBoardState))
                {
                    animalToTest.gridOriginPos = originalPos;
                    return false;
                }

                animalToTest.gridOriginPos = originalPos;
            }
        }
        return waitingAnimals.Count > 0;
    }
}

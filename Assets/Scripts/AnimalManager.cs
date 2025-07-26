using UnityEngine;

[ExecuteInEditMode]
public class AnimalManager : MonoBehaviour
{
    [Tooltip("Test etmek istediğiniz AnimalSO asset’leri (en az 2!)")]
    public AnimalSO[] animalDatas;

    private void OnValidate()
    {
        // Editörde inspector değiştiğinde
        CheckAllInteractions("OnValidate");
    }

    private void Start()
    {
        // Play tuşuna basıldığında
        CheckAllInteractions("Start");
    }

    [ContextMenu("Check All Interactions Now")]
    private void CheckAllInteractions(string caller = "ContextMenu")
    {
        // Başlangıç logu
        Debug.Log($"[AnimalManager] Çalıştırıldı: {caller}. Hayvan sayısı = {animalDatas?.Length ?? 0}");

        if (animalDatas == null || animalDatas.Length < 2)
        {
            Debug.LogWarning("[AnimalManager] Lütfen Inspector'da en az iki AnimalSO atayın ve her birinin traits dizisini doldurun.");
            return;
        }

        foreach (AnimalSO owner in animalDatas)
        {
            if (owner == null)
                continue;

            foreach (var trait in owner.traits)
            {
                if (trait == null)
                    continue;

                if (trait.antiToEveryTrait)
                {
                    CheckCollidingTraits(owner, trait);
                }
                else
                {
                    foreach (var antiTrait in trait.antiTraits)
                    {
                        CheckCollidingTraits(owner, trait, antiTrait);
                    }
                }
            }
        }
    }

    private bool containsAntiTrait(AnimalTraitSO antiTrait, AnimalSO otherAnimal)
    {
        foreach (var otherTrait in otherAnimal.traits)
            if (otherTrait == antiTrait) return true;
        
        return false;
    }

    private void CheckCollidingTraits(AnimalSO owner, AnimalTraitSO ownersTrait, AnimalTraitSO traitToCheck = null)
    {
        if (traitToCheck == null && !ownersTrait.antiToEveryTrait)
        {
            Debug.LogWarning($"[SPECIFIC] {owner._animalName}.{ownersTrait.traitName} içinde null bir antiTraits var.");
            return;
        }

        //print("Checking " + owner.name);
        Rect effectRect = ownersTrait.GetTraitEffectRect(owner.gridOriginPos, owner.size);

        foreach (AnimalSO otherAnimal in animalDatas)
        {
            if (otherAnimal == null || otherAnimal == owner) continue;

            if (!containsAntiTrait(traitToCheck, otherAnimal) && !ownersTrait.antiToEveryTrait)
            {
                //print($"compared {ownersTrait.name} to {traitToCheck.traitName}");
                continue;
            }

            Rect otherRect = new Rect(otherAnimal.gridOriginPos, otherAnimal.size);

            if (effectRect.Overlaps(otherRect))
            {
                Debug.Log($"[SPECIFIC] {owner._animalName} ({ownersTrait.traitName}) menzilinin icinde -> {otherAnimal._animalName}");
            }
        }
    }
}

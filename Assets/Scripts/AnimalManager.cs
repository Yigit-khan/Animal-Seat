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
    public bool IsAllInteractionsValid(List<AnimalSO> animalDatas)
    {
        _animalDatas = animalDatas;
        // Başlangıç logu
        Debug.Log($"[AnimalManager] Çalıştırıldı, hayvan sayısı = {_animalDatas?.Count ?? 0}");

        if (_animalDatas == null || _animalDatas.Count < 2)
        {
            Debug.LogWarning("[AnimalManager] Lütfen Inspector'da en az iki AnimalSO atayın ve her birinin traits dizisini doldurun.");
            return false;
        }

        foreach (AnimalSO owner in _animalDatas)
        {
            if (owner == null)
                continue;

            foreach (var trait in owner.traits)
            {
                if (trait == null)
                    continue;

                if (trait.antiToEveryTrait)
                {
                    if (IsPositionValid(owner, trait) == false)
                        return false;
                }
                else
                {
                    foreach (var antiTrait in trait.antiTraits)
                    {
                        if (antiTrait != null)
                            if (IsPositionValid(owner, trait, antiTrait) == false)
                                return false;
                    }
                }
            }
        }
        return true;
    }

    private bool IsPositionValid(AnimalSO owner, AnimalTraitSO ownersTrait, AnimalTraitSO traitToCheck = null)
    {
        if (traitToCheck == null && !ownersTrait.antiToEveryTrait)
        {
            Debug.LogWarning($"[SPECIFIC] {owner._animalName}.{ownersTrait.traitName} içinde null bir antiTraits var.");
            return false;
        }

        var effectPointList = ownersTrait.GetTraitEffectPoints(owner.gridOriginPos, owner.size);
        Debug.Log(owner._animalName + " hayvanı için point listesi: " + string.Join(", ", effectPointList));

        foreach (AnimalSO otherAnimal in _animalDatas)
        {
            if (otherAnimal == null || otherAnimal == owner)
                continue;
            if (!otherAnimal.traits.Contains(traitToCheck) && !ownersTrait.antiToEveryTrait)
            {
                //print("skipped: " + traitToCheck + " (" + otherAnimal.name +  "), " + ownersTrait);
                continue;
            }

            if (effectPointList.Contains(otherAnimal.gridOriginPos))
            {
                Debug.Log($"[SPECIFIC] {otherAnimal._animalName}, {owner._animalName}'nin menzilinin icinde ->  ({ownersTrait.traitName})");
                return false;
            }
        }
        return true;
    }
}

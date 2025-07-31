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
        //Debug.Log($"[AnimalManager] Çalıştırıldı, hayvan sayısı = {_animalDatas?.Count ?? 0}");

        if (_animalDatas == null || _animalDatas.Count < 2)
        {
            Debug.LogWarning("[AnimalManager] Lütfen Inspector'da en az iki AnimalSO atayın ve her birinin traits dizisini doldurun.");
            return false;
        }

        foreach (AnimalSO ownerAnimal in _animalDatas)
        {
            if (ownerAnimal == null)
                continue;
 
            if (IsPositionValid(ownerAnimal) == false)
                return false;
        }
        return true;
    }

    private bool IsPositionValid(AnimalSO ownerAnimal)
    {
        //if (ownerAnimal.gridOriginPos.x < 0 || ownerAnimal.gridOriginPos.y < 0)
        //{
        //    Debug.Log("eksi " + ownerAnimal.gridOriginPos.x + ", " + ownerAnimal.gridOriginPos.y);
        //    return false;
        //}

        //Debug.Log(ownerAnimal._animalName + " hayvanı için point listesi: " + string.Join(", ", effectPointList));

        foreach (AnimalSO otherAnimal in _animalDatas)
        {
            if (otherAnimal == null || otherAnimal == ownerAnimal) continue; // kendisiyle karşılaştırma yapma

            if (otherAnimal.gridOriginPos.x < 0 || otherAnimal.gridOriginPos.y < 0) continue;  // grid dışında ise geç

            if (otherAnimal.gridOriginPos == ownerAnimal.gridOriginPos) // üst üste gelemez
                return false;

            // Trait tabanlı etki alanı kontrolü
            foreach (var ownerTrait in ownerAnimal.traits)
            {
                // 1) Bu trait tüm trait'lere karşı ise:
                if (ownerTrait.antiToEveryTrait)
                {
                    var allPoints = ownerTrait.GetTraitEffectPoints(ownerAnimal.gridOriginPos, ownerAnimal.size);
                    if (allPoints.Contains(otherAnimal.gridOriginPos))
                        return false;
                }
                else
                {
                    // 2) Sadece spesifik antiTraits için:
                    var affectedPoints = ownerTrait.GetTraitEffectPoints(ownerAnimal.gridOriginPos, ownerAnimal.size);
                    foreach (var antiTrait in ownerTrait.antiTraits)
                    {
                        if (antiTrait != null && otherAnimal.traits.Contains(antiTrait))
                        {
                            if (affectedPoints.Contains(otherAnimal.gridOriginPos))
                                return false;
                        }
                    }
                }
            }

        }
        return true;
    }
}

using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimalTraitObject_", menuName = "Scriptable Objects/Animals/AnimalTraitSO")]
public class AnimalTraitSO : ScriptableObject
{
    [Tooltip("Hayvan trait'inin ismi")]
    public string traitName;

    [Tooltip("Trait iconunun yanında yer alacak açıklama")]
    public string traitDescription;

    [Tooltip("Anti traite karşı uygulanacak efektin etki menzili")]
    public Vector2Int traitEffectRange;

    [Tooltip("Bu trait'e karşı olan trait'lerin listesi")]
    public AnimalTraitSO[] antiTraits;

    [Tooltip("Yalnız oturmak ister (bütün trait'lere karşı anti)")]
    public bool antiToEveryTrait;

    public Rect GetTraitEffectRect(Vector2Int origin, Vector2Int size)
    {
        return new Rect(
            origin.x - traitEffectRange.x,
            origin.y - traitEffectRange.y,
            size.x + traitEffectRange.x * 2,
            size.y + traitEffectRange.y * 2
        );
    }
}

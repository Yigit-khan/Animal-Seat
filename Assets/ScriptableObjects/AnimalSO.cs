using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

[CreateAssetMenu(fileName = "AnimalObject_", menuName = "Scriptable Objects/Animals/AnimalSO")]

public class AnimalSO : ScriptableObject
{

    [Tooltip("Hayvanın ismi")]
    public string _animalName;

    [Tooltip("Hayvanın grid üzerinde bulunduğu başlangıç noktası (sol üst köşe)")]
    public Vector2Int gridOriginPos;
    
    [Tooltip("Hayvanın grid üzerindeki boyutu (genişlik, yükseklik)")]
    public Vector2Int size;

    [Tooltip("Hayvanın sahip olduğu karakteristiklerin (trait) listesi")]
    public List<AnimalTraitSO> traits;

    [HideInInspector] public bool effectedBySkill = false;

    //private void Awake()
    //{
    //    List<AnimalTraitSO> originalTraits = new List<AnimalTraitSO>(traits);

    //    traits.Clear();

    //    foreach (var trait in originalTraits)
    //    {
    //        traits.Add(ScriptableObject.Instantiate(trait));
    //    }
    //}
}

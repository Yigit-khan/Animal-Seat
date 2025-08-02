using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
public struct DirectionalRange
{
    [Tooltip("Yukarı olan etki menzili")]
    public int up;
    [Tooltip("Aşağı olan etki menzili")]
    public int down;
    [Tooltip("Sağ olan etki menzili")]
    public int right;
    [Tooltip("Sol olan etki menzili")]
    public int left;
}


[CreateAssetMenu(fileName = "AnimalTraitObject_", menuName = "Scriptable Objects/Animals/AnimalTraitSO")]
public class AnimalTraitSO : ScriptableObject
{
    [Tooltip("Hayvan trait'inin ismi")]
    public string traitName;

    [Tooltip("Trait iconunun yanında yer alacak açıklama")]
    public string traitDescription;

    [Tooltip("Trait iconu")]
    public Sprite traitIcon;

    [Tooltip("Yalnız oturmak ister (bütün trait'lere karşı anti)")]
    public bool antiToEveryTrait;

    [Tooltip("Bu trait'e karşı olan trait'lerin listesi")]
    public AnimalTraitSO[] antiTraits;

    [SerializeField] private DirectionalRange effectRanges;
    public Rect GetTraitEffectRect(Vector2Int origin, Vector2Int size)
    {
        float xMin = origin.x - effectRanges.left;
        float yMin = origin.y - effectRanges.down;
        float width = size.x + effectRanges.left + effectRanges.right;
        float height = size.y + effectRanges.down + effectRanges.up;
        return new Rect(xMin, yMin, width, height);
    }

    public List<Vector2Int> GetTraitEffectPoints(Vector2Int origin, Vector2Int size)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        for (int i = 1; i < effectRanges.up + 1; i++)
        {
            Vector2Int point = new Vector2Int(origin.x, origin.y + i);
            points.Add(point);
        }

        for (int i = 1; i < effectRanges.down + 1; i++)
        {
            Vector2Int point = new Vector2Int(origin.x, origin.y - i);
            points.Add(point);
        }

        for (int i = 1; i < effectRanges.right + 1; i++)
        {
            Vector2Int point = new Vector2Int(origin.x + i, origin.y);
            points.Add(point);
        }

        for (int i = 1; i < effectRanges.left + 1; i++)
        {
            Vector2Int point = new Vector2Int(origin.x - i, origin.y);
            points.Add(point);
        }

        return points;
    }

}
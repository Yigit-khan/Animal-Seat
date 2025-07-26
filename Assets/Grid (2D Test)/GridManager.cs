using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;  // Handles için
#endif

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    [Tooltip("Grid boyutu (genişlik x yükseklik)")]
    public Vector2Int gridSize = new Vector2Int(10, 10);

    [Tooltip("Her hücrenin boyutu (varsayılan 1)")]
    public float cellSize = 1f;

    [Tooltip("Sahnede test edilecek birden fazla AnimalSO asseti")]
    public AnimalSO[] animalDatas;

    [Tooltip("Her hayvan için otomatik oluşturulan rastgele renkler")]
    public Color[] animalColors;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (animalDatas != null)
        {
            if (animalColors == null || animalColors.Length != animalDatas.Length)
            {
                animalColors = new Color[animalDatas.Length];
                for (int i = 0; i < animalColors.Length; i++)
                    animalColors[i] = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.6f, 1f);
            }
        }
    }
#endif

    private void OnDrawGizmos()
    {
        if (gridSize.x <= 0 || gridSize.y <= 0) return;

        Gizmos.color = Color.gray;
        for (int x = 0; x < gridSize.x; x++)
            for (int y = 0; y < gridSize.y; y++)
            {
                var cellCenter = new Vector3(x + 0.5f, -y - 0.5f, 0) * cellSize;
                Gizmos.DrawWireCube(cellCenter, Vector3.one * cellSize);
            }

        if (animalDatas == null) return;

        for (int i = 0; i < animalDatas.Length; i++)
        {
            var data = animalDatas[i];
            if (data == null) continue;

            Color baseColor = (animalColors != null && i < animalColors.Length)
                              ? animalColors[i]
                              : Color.green;

            var origin = new Vector3(data.gridOriginPos.x, -data.gridOriginPos.y, 0);
            var center = origin + new Vector3(data.size.x / 2f, -data.size.y / 2f, 0);
            var size3d = new Vector3(data.size.x, data.size.y, 1) * cellSize;
            var worldCtr = center * cellSize;

            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.3f);
            Gizmos.DrawCube(worldCtr, size3d);

#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = Mathf.RoundToInt(cellSize * 10f); // hücre boyutuna göre ölçek
            Handles.Label(worldCtr, data._animalName, style);
#endif

            if (data.traits != null)
            {
                foreach (var trait in data.traits)
                {
                    if (trait == null) continue;
                    Rect r = trait.GetTraitEffectRect(data.gridOriginPos, data.size);
                    var rcCenter = new Vector3(r.x + r.width / 2f, -(r.y + r.height / 2f), 0) * cellSize;
                    var rcSize = new Vector3(r.width, r.height, 1) * cellSize;

                    Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
                    Gizmos.DrawWireCube(rcCenter, rcSize);
                }
            }
        }
    }
}

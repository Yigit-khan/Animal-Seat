using UnityEngine;

public class SeatController : MonoBehaviour
{
    // Artýk public gridX/gridY yerine Vector2Int kullanacaðýz.
    // Bu, grid koordinatlarýný tek bir deðiþkende tutar.
    public Vector2Int GridPosition { get; private set; }

    public bool isOccupied { get; private set; } = false;
    public AnimalController occupiedBy { get; private set; } = null;
    public bool isWet { get; set; } = false;
    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    // GridSystem bu metodu kullanarak koltuðun pozisyonunu ona bildirecek.

    void Awake()
    {
        // Oyun baþladýðýnda MeshRenderer'ý ve orijinal materyalini bulup kaydet.
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }
    }
    public void Highlight(Material highlightMaterial)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material = highlightMaterial;
        }
    }

    // Koltuðu orijinal rengine/materyaline geri döndürmek için.
    public void ResetHighlight()
    {
        if (meshRenderer != null && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }
    }
    public void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
    }

    public void Occupy(AnimalController animal)
    {
        isOccupied = true;
        occupiedBy = animal;
    }

    public void Vacate()
    {
        isOccupied = false;
        occupiedBy = null;
    }
}
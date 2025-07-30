using UnityEngine;

/// <summary>
/// Bir koltuðun davranýþýný, durumunu (dolu/boþ) ve grid üzerindeki pozisyonunu yönetir.
/// Seviye tasarýmý için oyun baþýnda üzerinde bir hayvanla baþlamasýný da saðlayabilir.
/// </summary>
public class SeatController : MonoBehaviour
{
    [Header("Seviye Tasarým Ayarý")]
    [Tooltip("Oyun baþýnda bu koltukta direkt olarak baþlayacak hayvan prefab'ýný buraya sürükleyin. Sadece o hayvanýn baþlangýç (sol üst) koltuðuna atayýn.")]
    public GameObject startingAnimalPrefab;

    [Header("Durum Bilgileri")]
    [Tooltip("Bu koltuðun grid sistemindeki koordinatý.")]
    public Vector2Int GridPosition;

    [Tooltip("Koltuk þu anda dolu mu?")]
    public bool isOccupied { get; private set; } = false;

    [Tooltip("Eðer koltuk doluysa, hangi hayvan tarafýndan iþgal edildiði.")]
    public AnimalController occupiedBy { get; private set; } = null;

    [Tooltip("Koltuk ýslak mý? (Gelecekteki mekanikler için)")]
    public bool isWet { get; set; } = false;


    // --- Özel Deðiþkenler ---
    private MeshRenderer meshRenderer;
    private Material originalMaterial;


    private void Awake()
    {
        // Oyun baþladýðýnda, görsel efektler için MeshRenderer'ý ve orijinal materyalini bulup kaydet.
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }
    }

    /// <summary>
    /// Koltuðun materyalini, verilen highlight materyali ile deðiþtirir.
    /// </summary>
    /// <param name="highlightMaterial">Uygulanacak yeni materyal.</param>
    public void Highlight(Material highlightMaterial)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material = highlightMaterial;
        }
    }

    /// <summary>
    /// Koltuðun materyalini oyun baþýnda sahip olduðu orijinal materyaline geri döndürür.
    /// </summary>
    public void ResetHighlight()
    {
        if (meshRenderer != null && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }
    }

    /// <summary>
    /// Bu koltuðun grid pozisyonunu ayarlar. Genellikle GridSystem tarafýndan çaðrýlýr.
    /// </summary>
    /// <param name="position">Grid üzerindeki yeni (x, y) pozisyonu.</param>
    public void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
    }

    /// <summary>
    /// Koltuðu belirli bir hayvan tarafýndan iþgal edilmiþ olarak iþaretler.
    /// </summary>
    /// <param name="animal">Koltuða yerleþen hayvanýn AnimalController'ý.</param>
    public void Occupy(AnimalController animal)
    {
        // Güvenlik kontrolü: Koltuk zaten doluysa uyarý ver.
        if (isOccupied)
        {
            Debug.LogWarning($"Koltuk [{GridPosition.x},{GridPosition.y}] zaten {occupiedBy.animalSO._animalName} tarafýndan dolu, ancak {animal.animalSO._animalName} yerleþtirilmeye çalýþýlýyor!");
            return;
        }

        isOccupied = true;
        occupiedBy = animal;

        // Kural sisteminin doðru çalýþmasý için hayvanýn ScriptableObject verisine
        // hangi koltuða (grid pozisyonuna) oturduðunu kaydet.
        if (animal != null && animal.animalSO != null)
        {
            animal.animalSO.gridOriginPos = this.GridPosition;
        }
    }

    /// <summary>
    /// Koltuðu boþaltýr ve iþgal durumunu sýfýrlar.
    /// </summary>
    public void Vacate()
    {
        isOccupied = false;
        occupiedBy = null;
    }
}
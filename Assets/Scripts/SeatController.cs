using UnityEngine;
using Unity.VisualScripting;
using System.Linq;



// Sadece Unity Editör'de çalışacak özel fonksiyonlar için bu using satırını ekliyoruz.
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Bir koltuğun davranışını, durumunu (dolu/boş) ve grid üzerindeki pozisyonunu yönetir.
/// Seviye tasarımı için oyun başında üzerinde bir hayvanla başlamasını da sağlayabilir.
/// </summary>
public class SeatController : MonoBehaviour
{
    [Header("Seviye Tasarım Ayarı")]
    [Tooltip("Oyun başında bu koltukta direkt olarak başlayacak hayvan prefab'ını buraya sürükleyin.")]
    public GameObject startingAnimalPrefab;

    [Header("Editör Görünüm Ayarları")]
    [Tooltip("Hayvan isminin görüneceği maksimum uzaklık. Sahnenin kalabalık olmasını engeller.")]
    [SerializeField] private float gizmoMaxDrawDistance = 30f;

    [Header("Durum Bilgileri")]
    [Tooltip("Bu koltuğun grid sistemindeki koordinatı.")]
    public Vector2Int GridPosition;

    [Tooltip("Koltuk şu anda dolu mu?")]
    public bool isOccupied
    {
        get;
        private set;

    }

        = false;

    [Tooltip("Eğer koltuk doluysa, hangi hayvan tarafından işgal edildiği.")]
    public AnimalController occupiedBy { get; private set; } = null;

    [Tooltip("Koltuk ıslak mı? (Gelecekteki mekanikler için)")]
    public bool isWet { get; set; } = false;


    // --- Özel Değişkenler ---
    private MeshRenderer meshRenderer;
    private Material originalMaterial;


    private void Awake()
    {
        // Oyun başladığında, görsel efektler için MeshRenderer'ı ve orijinal materyalini bulup kaydet.
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }
    }

    /// <summary>
    /// Koltuğun materyalini, verilen highlight materyali ile değiştirir.
    /// </summary>
    public void Highlight(Material highlightMaterial)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material = highlightMaterial;
        }
    }

    /// <summary>
    /// Koltuğun materyalini oyun başında sahip olduğu orijinal materyaline geri döndürür.
    /// </summary>
    public void ResetHighlight()
    {
        if (meshRenderer != null && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }
    }

    /// <summary>
    /// Bu koltuğun grid pozisyonunu ayarlar. Genellikle GridSystem tarafından çağrılır.
    /// </summary>
    public void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
    }

    /// <summary>
    /// Koltuğu belirli bir hayvan tarafından işgal edilmiş olarak işaretler.
    /// </summary>
    public void Occupy(AnimalController animal)
    {
        if (isOccupied)
        {
            Debug.LogWarning($"Koltuk [{GridPosition.x},{GridPosition.y}] zaten dolu!");
            return;
        }

        var tailRenderers = animal.GetComponentsInChildren<SkinnedMeshRenderer>().Where(renderer => renderer.name == "tail");
        foreach (var tailRenderer in tailRenderers)
        {
            tailRenderer.enabled = false;
        }

        isOccupied = true;
        occupiedBy = animal;
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
            boxCollider.enabled = false;

        if (animal != null && animal.animalSO != null)
        {
            animal.animalSO.gridOriginPos = this.GridPosition;
        }

        if (animal != null && animal.TryGetComponent<CapsuleCollider>(out var capsule))
            capsule.enabled = false;

        if (animal != null && animal.TryGetComponent<SphereCollider>(out var sphere))
            sphere.enabled = true;
    }

    /// <summary>
    /// Koltuğu boşaltır ve işgal durumunu sıfırlar.
    /// </summary>
    public void Vacate()
    {
        // Eğer dolu değilse veya referans yoksa, sadece koltuk collider'ını açıp çık
        if (!isOccupied || occupiedBy == null)
        {
            var seatCol0 = GetComponent<BoxCollider>();
            if (seatCol0 != null)
                seatCol0.enabled = true;
            isOccupied = false;
            occupiedBy = null;
            return;
        }

        var animal = occupiedBy;

        var tailRenderers = animal.GetComponentsInChildren<SkinnedMeshRenderer>().Where(renderer => renderer.name == "tail");
        foreach (var tailRenderer in tailRenderers)
        {
            tailRenderer.enabled = true;
        }

        // Koltuk collider'ını tekrar aç
        var seatCol = GetComponent<BoxCollider>();
        if (seatCol != null)
            seatCol.enabled = true;

        // Hayvanın colliderlarını eski haline getir: Capsule açık, Sphere kapalı
        if (animal.TryGetComponent<CapsuleCollider>(out var cap))
            cap.enabled = true;

        if (animal.TryGetComponent<SphereCollider>(out var sph))
            sph.enabled = false;

        isOccupied = false;
        occupiedBy = null;
    }


    // Bu fonksiyon bloğu sadece Unity Editör'de çalışır ve oyunun build'ine dahil edilmez.
#if UNITY_EDITOR
    /// <summary>
    /// Sadece Editör'de çalışır. Sahne penceresine görsel yardımcılar (Gizmos) çizmek için kullanılır.
    /// Bu fonksiyon, obje seçili olmasa bile her zaman çalışır.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Sadece bir prefab atanmışsa devam et.
        if (startingAnimalPrefab == null)
            return;

        // Sahne kamerasından çok uzaktaysak, performans ve okunabilirlik için çizim yapma.
        if (SceneView.currentDrawingSceneView != null)
        {
            float distanceToCamera = Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, transform.position);
            if (distanceToCamera > gizmoMaxDrawDistance)
                return;
        }

        // Atanan prefab'dan hayvanın ismini bul.
        AnimalController controller = startingAnimalPrefab.GetComponent<AnimalController>();
        string animalName = (controller != null && controller.animalSO != null)
                            ? controller.animalSO._animalName
                            : startingAnimalPrefab.name;

        // Metnin stilini ayarla (renk, boyut, kalınlık vb.).
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        // Metni koltuğun biraz üzerinde, sahnede çiz.
        Vector3 textPosition = transform.position + Vector3.up * 0.8f;
        Handles.Label(textPosition, animalName, style);
    }
#endif
}

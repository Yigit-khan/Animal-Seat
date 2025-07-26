using UnityEngine;
using UnityEditor; // Editör scriptleri için bu kütüphane gereklidir.

// Bu satýr, bu scriptin GameManager'ýn Inspector'ýný özelleþtireceðini belirtir.
[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. Mevcut Inspector'ý normal þekilde çiz.
        // Bu, GameManager'daki public deðiþkenlerin hala görünmesini saðlar.
        DrawDefaultInspector();

        // 2. Hedef script'e (GameManager) bir referans al.
        GameManager gameManager = (GameManager)target;

        // 3. Boþluk býrak ve butonumuzu ekle.
        EditorGUILayout.Space(20); // Biraz boþluk

        // Butonun görünümünü ayarla
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f); // Açýk yeþil bir renk
        if (GUILayout.Button("Koltuklarý Grid'e Yasla", GUILayout.Height(40)))
        {
            // Butona týklandýðýnda SnapSeatsToGrid fonksiyonunu çaðýr.
            SnapSeatsToGrid(gameManager);
        }
        GUI.backgroundColor = Color.white; // Rengi normale döndür
    }

    private void SnapSeatsToGrid(GameManager gm)
    {
        // GameManager'da referanslarýn atanýp atanmadýðýný kontrol et.
        Transform seatParent = gm.transform.Find("SeatParent"); // Bu referansý GameManager'dan almanýn daha iyi yollarý var ama bu basit bir baþlangýç.
        Transform gridOrigin = gm.transform.Find("_GridOrigin"); // Ayný þekilde

        // Asýl referanslarý serialized field üzerinden almak daha güvenli.
        // Bu yüzden GameManager'daki deðiþkenlere eriþeceðiz.
        // Ancak bu deðiþkenler private olduðu için reflection kullanmamýz gerekir.
        // Daha basit bir yol: Bu deðiþkenleri geçici olarak public yapabiliriz veya
        // GameManager'a bir public metot ekleyebiliriz.

        // En temiz yol: GameManager'a bir public metot eklemek.
        // Bu editör scriptinin doðrudan GameManager'ýn özel metotlarýný çaðýrmasýna izin verir.
        // Þimdilik, metodu doðrudan bu editör scripti içinde yazalým.

        var seatParentProp = serializedObject.FindProperty("seatParent");
        var gridOriginProp = serializedObject.FindProperty("gridOriginReference");
        var cellSizeProp = serializedObject.FindProperty("gridCellSize");

        if (seatParentProp.objectReferenceValue == null || gridOriginProp.objectReferenceValue == null)
        {
            Debug.LogError("Grid'e yaslama için 'Seat Parent' ve 'Grid Origin Reference' alanlarý GameManager'da atanmýþ olmalýdýr!");
            return;
        }

        Transform parent = (Transform)seatParentProp.objectReferenceValue;
        Transform origin = (Transform)gridOriginProp.objectReferenceValue;
        Vector2 cellSize = cellSizeProp.vector2Value;

        if (cellSize.x == 0 || cellSize.y == 0)
        {
            Debug.LogError("Grid Cell Size (X veya Y) 0 olamaz!");
            return;
        }

        int snappedCount = 0;
        // Parent'ýn altýndaki her bir 'çocuk' (koltuk) için döngüye gir.
        foreach (Transform seatTransform in parent)
        {
            // Adým A: Koltuðun dünya pozisyonunu grid koordinatýna çevir.
            Vector3 relativePos = seatTransform.position - origin.position;
            int gridX = Mathf.RoundToInt(relativePos.x / cellSize.x);
            int gridY = Mathf.RoundToInt(relativePos.z / cellSize.y); // Dünya Z'si bizim grid Y'miz.

            // Adým B: Bu grid koordinatýnýn dünya pozisyonunu hesapla.
            Vector3 targetPosition = origin.position + new Vector3(gridX * cellSize.x, 0, gridY * cellSize.y);

            // Adým C: Koltuðun kendi Y pozisyonunu koruyarak yeni pozisyonunu ata.
            // Bu, koltuklarýn farklý yüksekliklerde olabilmesine olanak tanýr.
            seatTransform.position = new Vector3(targetPosition.x, seatTransform.position.y, targetPosition.z);
            snappedCount++;
        }

        Debug.Log($"{snappedCount} adet koltuk baþarýyla grid'e yaslandý!");
    }
}
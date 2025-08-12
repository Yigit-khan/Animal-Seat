using UnityEngine;
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);
    private Vector2Int lastScreenSize = new Vector2Int(0, 0);
    private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        // Eðer ekran dönerse veya çözünürlük deðiþirse yeniden uygula
        if (lastSafeArea != Screen.safeArea
            || lastScreenSize.x != Screen.width
            || lastScreenSize.y != Screen.height
            || lastOrientation != Screen.orientation)
        {
            ApplySafeArea();
        }
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // Oranlarý hesapla (0 - 1 arasý)
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Anchorlarý güncelle
        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;

        // Son deðerleri sakla
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        lastOrientation = Screen.orientation;
    }
}

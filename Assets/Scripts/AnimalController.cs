using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour
{
    [Tooltip("Bu hayvanýn tüm verilerini ve karakteristiklerini tutan ScriptableObject.")]
    public AnimalSO animalSO;

    // --- YENÝ DEÐÝÞKEN ---
    [Tooltip("Bu hayvanýn bir koltuða oturup oturmadýðýný belirtir.")]
    public bool isSeated = false; // Varsayýlan olarak false.

    public int originalLayer { get; private set; }
    private List<GameObject> myBubbles = new List<GameObject>();

    public List<SeatController> occupiedSeats = new List<SeatController>();
    public void Initialize()
    {
        originalLayer = gameObject.layer;
    }

    /// <summary>
    /// Bu hayvanýn sahip olduðu tüm karakteristikler (trait) için düþünce balonlarý oluþturur.
    /// </summary>
    public void DisplayMyRules()
    {
        // 1. KONTROL: Eðer hayvan "oturuyor" olarak iþaretlenmiþse, ASLA balon oluþturma.
        if (isSeated)
        {
            ClearMyBubbles(); // Hatta varsa eski balonlarý da sil.
            return;           // Fonksiyondan hemen çýk.
        }

        // Önceki balonlarý temizle
        ClearMyBubbles();

        if (animalSO == null || animalSO.traits == null) return;

        // Balon oluþturma mantýðý (sadece oturmayan hayvanlar için çalýþacak)
        foreach (var trait in animalSO.traits)
        {
            Sprite icon = GameManager.Instance.GetIconForTrait(trait);
            if (icon != null)
            {
                GameObject bubbleObj = GameManager.Instance.CreateThoughtBubble(icon, trait.traitDescription);
                if (bubbleObj != null)
                {
                    bubbleObj.transform.SetParent(this.transform);
                    bubbleObj.transform.localPosition = GameManager.Instance.bubbleOffset;
                    myBubbles.Add(bubbleObj);
                }
            }
        }
    }

    /// <summary>
    /// Hayvanýn üzerindeki tüm düþünce balonlarýný siler.
    /// </summary>
    public void ClearMyBubbles()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<ThoughtBubbleController>() != null)
            {
                Destroy(child.gameObject);
            }
        }
        myBubbles.Clear();
    }

    void OnDestroy()
    {
        ClearMyBubbles();
    }
}
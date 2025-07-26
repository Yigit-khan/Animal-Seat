using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour
{
    public AnimalData data { get; private set; }
    public int originalLayer { get; private set; }

    private List<GameObject> myBubbles = new List<GameObject>(); // Kendi balonlarýný tutar

    public void Initialize(AnimalData animalData)
    {
        data = animalData;
        originalLayer = gameObject.layer;

        // Hayvan oluþturulur oluþturulmaz kendi kurallarýný gösterir.
        DisplayMyRules();
    }

    public void DisplayMyRules()
    {
        // Önce varsa eski balonlarý temizle.
        ClearMyBubbles();

        List<RuleIconData> applicableRules = GameManager.Instance.GetRulesForAnimal(data.turu);

        foreach (var rule in applicableRules)
        {
            // GameManager'dan balonu oluþturmasýný iste.
            // Artýk geriye dönen GameObject'i direkt kullanacaðýz.
            GameObject bubbleObj = GameManager.Instance.CreateThoughtBubble(rule.Icon, rule.Description);

            if (bubbleObj != null)
            {
                // --- ÝÞTE KRÝTÝK DEÐÝÞÝKLÝK ---
                // 1. Balonun parent'ýný bu hayvan objesi yap.
                bubbleObj.transform.SetParent(this.transform);

                // 2. Balonun yerel pozisyonunu ayarla (hayvanýn kendi merkezine göre).
                // GameManager'daki bubbleOffset'i buraya taþýyabilir veya direkt burada belirleyebiliriz.
                bubbleObj.transform.localPosition = new Vector3(0, 1.5f, 0); // Hayvanýn 1.5 birim üzerinde duracak.

                myBubbles.Add(bubbleObj);
            }
        }
    }

    // Hayvan sahneden kaldýrýldýðýnda (örn: koltuða oturduðunda), balonlarýný da temizle.
    public void ClearMyBubbles()
    {
        foreach (var bubble in myBubbles)
        {
            if (bubble != null)
            {
                Destroy(bubble);
            }
        }
        myBubbles.Clear();
    }

    void OnDestroy()
    {
        // Her ihtimale karþý, obje yok olurken balonlarýný da temizle.
        ClearMyBubbles();
    }
}
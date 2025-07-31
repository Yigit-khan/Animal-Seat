using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class AnimalManager 
{
    private List<AnimalSO> _animalDatas;


    [ContextMenu("Check All Interactions Now")]

    public bool IsAllInteractionsValid(List<AnimalSO> animalDatas)
    {
        _animalDatas = animalDatas;
        // Başlangıç logu
        //Debug.Log($"[AnimalManager] Çalıştırıldı, hayvan sayısı = {_animalDatas?.Count ?? 0}");

        if (_animalDatas == null || _animalDatas.Count < 2)
        {
            Debug.LogWarning("[AnimalManager] Lütfen Inspector'da en az iki AnimalSO atayın ve her birinin traits dizisini doldurun.");
            return false;
        }

        foreach (AnimalSO ownerAnimal in _animalDatas)
        {
            if (ownerAnimal == null)
                continue;
 
            if (IsPositionValid(ownerAnimal) == false)
                return false;
        }
        return true;
    }

    private bool IsPositionValid(AnimalSO ownerAnimal)
    {
        //if (ownerAnimal.gridOriginPos.x < 0 || ownerAnimal.gridOriginPos.y < 0)
        //{
        //    Debug.Log("eksi " + ownerAnimal.gridOriginPos.x + ", " + ownerAnimal.gridOriginPos.y);
        //    return false;
        //}

        //Debug.Log(ownerAnimal._animalName + " hayvanı için point listesi: " + string.Join(", ", effectPointList));

        foreach (AnimalSO otherAnimal in _animalDatas)
        {
            if (otherAnimal == null || otherAnimal == ownerAnimal) continue; // kendisiyle karşılaştırma yapma

            if (otherAnimal.gridOriginPos.x < 0 || otherAnimal.gridOriginPos.y < 0) continue;  // grid dışında ise geç

            if (otherAnimal.gridOriginPos == ownerAnimal.gridOriginPos) // üst üste gelemez
                return false;

            // Trait tabanlı etki alanı kontrolü
            foreach (var ownerTrait in ownerAnimal.traits)
            {
                // 1) Bu trait tüm trait'lere karşı ise:
                if (ownerTrait.antiToEveryTrait)
                {
                    var allPoints = ownerTrait.GetTraitEffectPoints(ownerAnimal.gridOriginPos, ownerAnimal.size);
                    if (allPoints.Contains(otherAnimal.gridOriginPos))
                        return false;
                }
                else
                {
                    // 2) Sadece spesifik antiTraits için:
                    var affectedPoints = ownerTrait.GetTraitEffectPoints(ownerAnimal.gridOriginPos, ownerAnimal.size);
                    foreach (var antiTrait in ownerTrait.antiTraits)
                    {
                        if (antiTrait != null && otherAnimal.traits.Contains(antiTrait))
                        {
                            if (affectedPoints.Contains(otherAnimal.gridOriginPos))
                                return false;
                        }
                    }
                }
            }

        }
        return true;
    }

    // AnimalManager.cs içine eklenecek YENİ fonksiyon

    /// <summary>
    /// Verilen bekleme listesindeki herhangi bir hayvanın, verilen boş koltuklardan herhangi birine
    /// geçerli bir şekilde yerleştirilip yerleştirilemeyeceğini kontrol eder.
    /// </summary>
    /// <param name="waitingAnimals">Kuyrukta bekleyen hayvanların SO'ları.</param>
    /// <param name="emptySeats">Tahtadaki tüm boş koltuklar.</param>
    /// <param name="seatedAnimals">Tahtada hali hazırda oturan hayvanların SO'ları.</param>
    /// <returns>Yerleştirilecek bir hamle varsa 'false' (kilitli değil), yoksa 'true' (kilitli) döner.</returns>
    public bool IsSoftLocked(List<AnimalSO> waitingAnimals, List<SeatController> emptySeats, List<AnimalSO> seatedAnimals)
    {
        // 1. Olası hamle için hiç boş koltuk yoksa, kesinlikle kilitlenmiştir.
        if (emptySeats == null || emptySeats.Count == 0)
        {
            // Not: Burada boş bekleme slotu olup olmadığını da ayrıca kontrol etmek gerekebilir,
            // ama şimdilik sadece ana koltuklara odaklanıyoruz.
            return true;
        }

        // Her bir bekleyen hayvanı...
        foreach (var animalToTest in waitingAnimals)
        {
            // ...her bir boş koltuğa yerleştirmeyi simüle et.
            foreach (var seatToTest in emptySeats)
            {
                // --- SİMÜLASYON BAŞLANGICI ---

                // A) Kural kontrolü için tahtanın geçici bir kopyasını oluştur.
                // Bu kopya, oturan hayvanları VE test ettiğimiz hayvanı içerir.
                var hypotheticalBoardState = new List<AnimalSO>(seatedAnimals);
                hypotheticalBoardState.Add(animalToTest);

                // B) Hayvanın pozisyonunu geçici olarak değiştir.
                Vector2Int originalPos = animalToTest.gridOriginPos;
                animalToTest.gridOriginPos = seatToTest.GridPosition;

                // C) Kural motorunu bu geçici durum için ayarla ve kontrol et.
                //_animalDatas = hypotheticalBoardState;
                if (IsAllInteractionsValid(hypotheticalBoardState))
                {
                    // GEÇERLİ BİR HAMLE BULUNDU!
                    Debug.Log($"[Soft-Lock Check] GEÇERLİ HAMLE: '{animalToTest._animalName}' hayvanı [{seatToTest.GridPosition}] pozisyonuna yerleştirilebilir.");

                    // Simülasyonu temizle ve kilitlenme olmadığını bildir.
                    animalToTest.gridOriginPos = originalPos;
                    return false; // false -> Kilitlenme YOK.
                }

                // --- SİMÜLASYON SONU ---
                // Bu hamle geçerli değildi, bir sonrakini denemeden önce pozisyonu sıfırla.
                animalToTest.gridOriginPos = originalPos;
            }
        }

        // Eğer tüm döngüler bitti ve hiçbir geçerli hamle bulunamadıysa...
        Debug.Log("[Soft-Lock Check] KİLİTLENDİ: Bekleyen hiçbir hayvan hiçbir boş koltuğa yerleştirilemiyor.");
        return true; // true -> Kilitlenme VAR.
    }
}

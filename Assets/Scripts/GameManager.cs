using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Seviye Verileri")]
    [SerializeField] private List<AnimalData> currentLevelAnimals;

    [Header("Sahne ve Prefab Referanslarý")]
    [SerializeField] private GameObject animalPrefab3D;
    [SerializeField] private LayerMask seatLayer;
    [SerializeField] private LayerMask animalLayer;

    [Header("Grid Sistemi Ayarlarý")]
    [SerializeField] private Transform seatParent;
    [SerializeField] private Transform gridOriginReference;
    [SerializeField] private Vector2 gridCellSize = new Vector2(1.2f, 1.2f);

    [Header("Fiziksel Kuyruk ve Sürükleme Ayarlarý")]
    [SerializeField] private Transform[] queuePositions;
    [SerializeField] private float dragLiftHeight = 1.5f;
    [SerializeField] private float queueMoveSpeed = 8f;
    [SerializeField] private float seatHeightOffset = 0.1f;
    [SerializeField] private float dragZOffset = -2f;

    [Header("Düþünce Balonu Ayarlarý")]
    [SerializeField] private GameObject thoughtBubblePrefab;
    [SerializeField] private List<RuleIconData> allRuleIcons;
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0, 1.5f, 0);

    [Header("Can Sistemi Ayarlarý")]
    [Tooltip("Oyuncunun baþlangýçtaki can sayýsý.")]
    [SerializeField] private int maxLives = 5;
    [Tooltip("Can ikonlarýnýn oluþturulacaðý UI parent'ý.")]
    [SerializeField] private Transform heartsContainer;
    [Tooltip("Bir caný temsil eden UI prefab'ý.")]
    [SerializeField] private GameObject heartIconPrefab;

    [Header("Bekleme Koltuðu Ayarlarý")]
    // Bu diziye artýk gerek yok, çünkü slotlarý kodla oluþturacaðýz.
    // [SerializeField] private HoldingSlotController[] holdingSlots; 
    [Tooltip("Bekleme slotlarýnýn oluþturulacaðý parent obje.")]
    [SerializeField] private Transform holdingSlotsParent;
    [Tooltip("Normal, kilitsiz bekleme koltuðu prefab'ý.")]
    [SerializeField] private GameObject holdingSlotPrefab;
    [Tooltip("Kilitli bekleme koltuðu prefab'ý.")]
    [SerializeField] private GameObject lockedSlotPrefab;
    [Tooltip("Toplam kaç adet bekleme koltuðu olacak.")]
    [SerializeField] private int totalHoldingSlots = 4;
    [Tooltip("Baþlangýçta kaç adet koltuðun kilitsiz olacaðý.")]
    [SerializeField] private int unlockedSlotsAtStart = 2;
    [Tooltip("Slotlar arasýndaki dikey mesafe.")]
    [SerializeField] private float holdingSlotSpacing = 2.0f;
    private List<HoldingSlotController> holdingSlots = new List<HoldingSlotController>();
    [SerializeField] private LayerMask holdingSlotLayer; // Hata 1'in çözümü

    [Header("Etki Alaný Gösterme Ayarlarý")]
    [Tooltip("Etki alanýndaki koltuklarý renklendirmek için kullanýlacak materyal.")]
    [SerializeField] private Material effectAreaMaterial;

    // --- Özel Deðiþkenler ---
    private List<SeatController> currentlyHighlightedSeats = new List<SeatController>();

    // --- Sistemler ve Özel Deðiþkenler ---
    private GridSystem gridSystem;
    private List<AnimalController> animalQueue = new List<AnimalController>();
    private AnimalController selectedAnimal = null;
    private Plane dragPlane;
    private Vector3 offset;
    private int currentLives;
    private List<GameObject> heartIcons = new List<GameObject>();

    private Transform startParentOfSelectedAnimal;

    private SeatController lastValidSeatTarget = null;
    private HoldingSlotController lastValidHoldingSlotTarget = null; // YENÝ
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f; // Oyun yeniden baþladýðýnda zamaný normale döndür.
        gridSystem = new GridSystem(seatParent, gridOriginReference, gridCellSize);
        SetupHoldingSlots();
        SetupLives();
        InitializeAnimalQueue();
    }

    private void Update()
    {
        UpdateAnimalQueuePositions();
        HandlePlayerInput();
    }

    #region Public Fonksiyonlar

    public List<RuleIconData> GetRulesForAnimal(AnimalType animalType)
    {
        List<RuleIconData> rules = new List<RuleIconData>();
        foreach (var rule in allRuleIcons)
        {
            if (rule.SourceAnimal == animalType && rule.Interaction == InteractionType.Dislikes)
            {
                rules.Add(rule);
            }
        }
        return rules;
    }

    public GameObject CreateThoughtBubble(Sprite icon, string description)
    {
        if (thoughtBubblePrefab == null) return null;
        GameObject bubbleObj = Instantiate(thoughtBubblePrefab);
        if (bubbleObj.TryGetComponent<ThoughtBubbleController>(out var controller))
        {
            controller.Initialize(icon, description);
        }
        return bubbleObj;
    }

    #endregion

    #region Özel Fonksiyonlar

    private void SetupLives()
    {
        currentLives = maxLives;
        foreach (Transform child in heartsContainer) Destroy(child.gameObject);
        heartIcons.Clear();
        for (int i = 0; i < maxLives; i++)
        {
            GameObject heart = Instantiate(heartIconPrefab, heartsContainer);
            heartIcons.Add(heart);
        }
    }

    private void InitializeAnimalQueue()
    {
        for (int i = 0; i < currentLevelAnimals.Count; i++)
        {
            if (i < queuePositions.Length)
            {
                var animalController = Instantiate(animalPrefab3D, queuePositions[i].position, Quaternion.identity)
                    .GetComponent<AnimalController>();
                animalController.Initialize(currentLevelAnimals[i]);
                animalQueue.Add(animalController);
            }
        }
    }

    private void UpdateAnimalQueuePositions()
    {
        for (int i = 0; i < animalQueue.Count; i++)
        {
            if (animalQueue[i] != selectedAnimal)
            {
                animalQueue[i].transform.position = Vector3.Lerp(
                    animalQueue[i].transform.position,
                    queuePositions[i].position,
                    Time.deltaTime * queueMoveSpeed
                );
            }
        }
    }

    private void HandlePlayerInput()
    {
        if (Input.GetMouseButtonDown(0)) HandleMouseDown();
        if (Input.GetMouseButton(0) && selectedAnimal != null) HandleMouseDrag();
        if (Input.GetMouseButtonUp(0) && selectedAnimal != null) HandleMouseUp();
    }

    private void HandleMouseDown()
    {
        // Eðer zaten bir hayvan seçiliyse, yeni bir týklama iþlemi yapma.
        if (selectedAnimal != null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 1. Bekleme koltuðundaki bir hayvana mý týklandý?
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, animalLayer))
        {
            foreach (var slot in holdingSlots)
            {
                if (slot.CurrentState == SlotState.Occupied && slot.OccupyingAnimal != null && slot.OccupyingAnimal.gameObject == hit.collider.gameObject)
                {
                    selectedAnimal = slot.PickUpAnimal().Animal;
                    startParentOfSelectedAnimal = slot.transform;
                    StartDraggingSelectedAnimal();
                    return;
                }
            }
        }

        // 2. Kuyruktaki hayvana mý týklandý?
        if (Physics.Raycast(ray, out hit, 100f, animalLayer))
        {
            if (animalQueue.Count > 0 && hit.collider.gameObject == animalQueue[0].gameObject)
            {
                selectedAnimal = animalQueue[0];
                startParentOfSelectedAnimal = null;
                StartDraggingSelectedAnimal();
                return;
            }
        }

        // 3. Kilitli bir slota mý týklandý?
        if (Physics.Raycast(ray, out hit, 100f, holdingSlotLayer))
        {
            if (hit.collider.TryGetComponent<HoldingSlotController>(out var slotController) && slotController.CurrentState == SlotState.Locked)
            {
                TryUnlockSlot(slotController);
            }
        }
    }

    private void HandleMouseDrag()
    {
        // 1. Hayvanýn Pozisyonunu Sürükleyerek Güncelle
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(mouseRay, out float enter))
        {
            Vector3 targetPosition = mouseRay.GetPoint(enter) + offset;
            targetPosition.y = dragLiftHeight;
            selectedAnimal.transform.position = targetPosition;
        }

        // 2. Hedef Tespiti ve Görsel Geri Bildirim
        Ray downwardRay = new Ray(selectedAnimal.transform.position, Vector3.down);
        bool foundTarget = false;

        // Önce, hayvanýn altýnda bir ANA KOLTUK var mý?
        if (Physics.Raycast(downwardRay, out RaycastHit hit, 20f, seatLayer))
        {
            if (hit.collider.TryGetComponent<SeatController>(out SeatController targetSeat))
            {
                foundTarget = true;
                lastValidHoldingSlotTarget = null; // Diðer hedefi temizle
                if (lastValidSeatTarget != targetSeat)
                {
                    ShowEffectArea(selectedAnimal.data, targetSeat);
                    lastValidSeatTarget = targetSeat;
                }
            }
        }

        // Eðer ana koltuk bulunamadýysa, BEKLEME KOLTUÐU var mý?
        if (!foundTarget && Physics.Raycast(downwardRay, out hit, 20f, holdingSlotLayer))
        {
            if (hit.collider.TryGetComponent<HoldingSlotController>(out HoldingSlotController targetHoldingSlot))
            {
                if (targetHoldingSlot.CurrentState == SlotState.Unlocked)
                {
                    foundTarget = true;
                    ResetAllHighlights(); // Etki alaný yok
                    lastValidSeatTarget = null; // Diðer hedefi temizle
                    lastValidHoldingSlotTarget = targetHoldingSlot;

                    // Ýsteðe baðlý: Bekleme koltuðu için de bir highlight efekti eklenebilir.
                    // targetHoldingSlot.Highlight();
                }
            }
        }

        // Eðer hiçbir hedefin üzerinde deðilse...
        if (!foundTarget)
        {
            ResetAllHighlights();
            lastValidSeatTarget = null;
            lastValidHoldingSlotTarget = null;
        }
    }

    private void HandleMouseUp()
    {
        if (selectedAnimal == null) return;
        ResetAllHighlights();

        bool placedSuccessfully = false;

        // 1. ÖNCELÝK: Hafýzada geçerli bir ana koltuk hedefi var mý?
        if (lastValidSeatTarget != null)
        {
            placedSuccessfully = TryPlaceOnSeat(lastValidSeatTarget);
        }
        // 2. ÖNCELÝK: Hafýzada geçerli bir bekleme koltuðu hedefi var mý?
        else if (lastValidHoldingSlotTarget != null)
        {
            placedSuccessfully = TryPlaceOnHoldingSlot(lastValidHoldingSlotTarget);
        }

        // 3. Eðer hiçbir yere yerleþemediyse, orijinal pozisyonuna geri dön.
        if (!placedSuccessfully)
        {
            ReturnAnimalToOrigin();
        }

        selectedAnimal.gameObject.layer = selectedAnimal.originalLayer;
        selectedAnimal = null;
        lastValidSeatTarget = null;
        lastValidHoldingSlotTarget = null;
    }

    private bool TryPlaceOnSeat(SeatController targetSeat)
    {
        // Hedefin geçerli olduðundan emin ol (güvenlik kontrolü).
        if (targetSeat == null) return false;

        // Kural kontrolünü doðrudan bu hedefe göre yap.
        string validationError = IsPlacementValid(selectedAnimal.data, targetSeat);
        if (!string.IsNullOrEmpty(validationError))
        {
            Debug.LogWarning("KURAL ÝHLALÝ: " + validationError);
            LoseLife();
            return false; // Yerleþtirme baþarýsýz.
        }

        // Kurallar uygunsa, hayvaný bu hedefe yerleþtir.
        PlaceAnimalOnSeat(selectedAnimal, targetSeat);
        return true; // Yerleþtirme baþarýlý.
    }

    private void PlaceAnimalOnSeat(AnimalController animal, SeatController seat)
    {
        animal.ClearMyBubbles();
        animal.transform.position = seat.transform.position + new Vector3(0, seatHeightOffset, 0);
        seat.Occupy(animal);
        animalQueue.Remove(animal);
        animal.gameObject.layer = animal.originalLayer;
        CheckWinCondition();
    }

    private string IsPlacementValid(AnimalData animalToPlace, SeatController targetSeat)
    {
        if (targetSeat.isOccupied) return "Bu koltuk zaten dolu!";

        if (animalToPlace.kapladigiKoltukSayisi > 1)
        {
            SeatController adjacentSeat = gridSystem.GetSeatAt(targetSeat.GridPosition + Vector2Int.right);
            if (adjacentSeat == null || adjacentSeat.isOccupied) return "Fil'in sýðmasý için yandaki koltuk boþ deðil!";
        }

        if (animalToPlace.turu == AnimalType.Yirtici)
        {
            foreach (var neighbor in gridSystem.GetNeighbors(targetSeat, 1))
            {
                if (neighbor.isOccupied && neighbor.occupiedBy.data.turu == AnimalType.Otobur)
                    return "Yýrtýcý, otoburun yanýna oturamaz!";
            }
        }

        if (animalToPlace.turu == AnimalType.Otobur)
        {
            foreach (var neighbor in gridSystem.GetNeighbors(targetSeat, 1))
            {
                if (neighbor.isOccupied && neighbor.occupiedBy.data.turu == AnimalType.Yirtici)
                    return "Otobur, yýrtýcýnýn hemen yanýna oturamaz!";
            }
        }

        // Diðer kurallar buraya eklenebilir...

        return string.Empty; // Hiçbir kural ihlal edilmedi.
    }

    private void LoseLife()
    {
        if (currentLives <= 0) return;
        currentLives--;
        if (heartIcons.Count > 0)
        {
            Destroy(heartIcons[heartIcons.Count - 1]);
            heartIcons.RemoveAt(heartIcons.Count - 1);
        }
        if (currentLives <= 0) GameOver();
    }

    private void GameOver()
    {
        Debug.LogError("OYUN BÝTTÝ! Tüm canlarýný kaybettin.");
        Time.timeScale = 0f;
        // TODO: "Tekrar Dene" UI panelini göster.
    }

    private void CheckWinCondition()
    {
        if (animalQueue.Count == 0)
        {
            Debug.Log("TEBRÝKLER! SEVÝYE TAMAMLANDI!");
            // TODO: "Seviye Geçildi" UI panelini göster.
        }
    }
    #endregion

    #region Unity Editor Gizmos
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying && (seatParent == null || gridOriginReference == null)) return;
        if (gridSystem == null) gridSystem = new GridSystem(seatParent, gridOriginReference, gridCellSize);
        if (gridOriginReference != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(gridOriginReference.position, 0.3f);
        }
        Dictionary<Vector2Int, SeatController> gridData = gridSystem.GetFullGrid();
        if (gridData == null) return;
        foreach (var pair in gridData)
        {
            Gizmos.color = (Application.isPlaying && pair.Value.isOccupied) ? Color.red : Color.cyan;
            Gizmos.DrawWireCube(pair.Value.transform.position, new Vector3(gridCellSize.x, 0.5f, gridCellSize.y));
            Handles.Label(pair.Value.transform.position + Vector3.up * 0.5f, $"[{pair.Key.x},{pair.Key.y}]");
        }
    }
#endif
    #endregion

    private void SetupHoldingSlots()
    {
        // Önce eski slotlarý temizle (seviye yeniden baþlarsa)
        foreach (Transform child in holdingSlotsParent)
        {
            Destroy(child.gameObject);
        }
        holdingSlots.Clear();

        // Toplam slot sayýsý kadar döngüye gir.
        for (int i = 0; i < totalHoldingSlots; i++)
        {
            // --- DEÐÝÞÝKLÝK BURADA ---
            // Pozisyonu X eksenine göre hesapla (yan yana dizilecekler)
            Vector3 position = holdingSlotsParent.position + new Vector3(i * holdingSlotSpacing, 0, 0);

            // Geri kalan mantýk ayný.

            // Eðer bu slot kilitli olacaksa...
            if (i >= unlockedSlotsAtStart)
            {
                // Kilitli prefab'ý oluþtur.
                GameObject slotObj = Instantiate(lockedSlotPrefab, position, Quaternion.identity, holdingSlotsParent);
                HoldingSlotController controller = slotObj.GetComponent<HoldingSlotController>();
                if (controller != null)
                {
                    controller.Initialize(SlotState.Locked);
                    holdingSlots.Add(controller);
                }
            }
            // Eðer bu slot açýk olacaksa...
            else
            {
                // Normal prefab'ý oluþtur.
                GameObject slotObj = Instantiate(holdingSlotPrefab, position, Quaternion.identity, holdingSlotsParent);
                HoldingSlotController controller = slotObj.GetComponent<HoldingSlotController>();
                if (controller != null)
                {
                    controller.Initialize(SlotState.Unlocked);
                    holdingSlots.Add(controller);
                }
            }
        }
    }

    private void StartDraggingSelectedAnimal()
    {
        if (selectedAnimal == null) return;

        selectedAnimal.ClearMyBubbles();
        selectedAnimal.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // Sürükleme düzlemini Y ekseninde, kaldýrma yüksekliðinde oluþtur.
        dragPlane = new Plane(Vector3.up, new Vector3(0, dragLiftHeight, 0));

        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(mouseRay, out float enter))
        {
            // Hayvanýn baþlangýç pozisyonunu al.
            Vector3 animalStartPosition = selectedAnimal.transform.position;

            // Mouse'un düzlemdeki baþlangýç noktasýný al.
            Vector3 planeHitPoint = mouseRay.GetPoint(enter);

            // Hem Y hem de Z ofsetini tek seferde hesapla.
            // Hayvanýn son pozisyonu = (Düzlemdeki Nokta + Z Ofseti) + (Baþlangýçtaki Fark)
            // Offset = Baþlangýç Pozisyonu - (Düzlemdeki Nokta + Z Ofseti)
            offset = animalStartPosition - (planeHitPoint + new Vector3(0, 0, dragZOffset));
        }
    }
    private void TryUnlockSlot(HoldingSlotController lockedSlot)
    {
        Debug.Log("Kilitli slota týklandý! Kilit açýlýyor...");

        // TODO: Para kontrolü mantýðý buraya eklenebilir.
        // if (playerCoins < unlockCost) return;
        // playerCoins -= unlockCost;

        // 1. Mevcut kilitli slotun pozisyonunu ve parent'ýný kaydet.
        Vector3 position = lockedSlot.transform.position;
        Transform parent = lockedSlot.transform.parent;
        int index = holdingSlots.IndexOf(lockedSlot); // Listenin neresinde olduðunu bul

        // 2. Eski kilitli slot objesini yok et.
        holdingSlots.Remove(lockedSlot);
        Destroy(lockedSlot.gameObject);

        // 3. Ayný yere yeni, normal bir slot oluþtur.
        GameObject newSlotObj = Instantiate(holdingSlotPrefab, position, Quaternion.identity, parent);
        HoldingSlotController newController = newSlotObj.GetComponent<HoldingSlotController>();
        newController.Initialize(SlotState.Unlocked);

        // 4. Yeni slotu, listenin ayný sýrasýna ekle.
        if (index != -1)
        {
            holdingSlots.Insert(index, newController);
        }
        else
        {
            holdingSlots.Add(newController); // Güvenlik önlemi
        }
    }
    private bool TryPlaceOnHoldingSlot(HoldingSlotController targetSlot)
    {
        // Hedefin geçerli olduðundan emin ol.
        if (targetSlot == null || targetSlot.CurrentState != SlotState.Unlocked)
        {
            return false;
        }

        // Hayvaný bu hedefe yerleþtir.
        targetSlot.PlaceAnimal(selectedAnimal);
        selectedAnimal.transform.position = targetSlot.transform.position + new Vector3(0, seatHeightOffset, 0);
        animalQueue.Remove(selectedAnimal);
        return true;
    }

    private void ReturnAnimalToOrigin()
    {
        // Eðer hayvan bir bekleme slotundan alýndýysa...
        if (startParentOfSelectedAnimal != null && startParentOfSelectedAnimal.TryGetComponent<HoldingSlotController>(out var slot))
        {
            // Onu tekrar ayný slota geri koy.
            slot.PlaceAnimal(selectedAnimal);
            selectedAnimal.transform.position = slot.transform.position + new Vector3(0, seatHeightOffset, 0);
        }
        // Eðer hayvan kuyruktan alýndýysa...
        else
        {
            // Kuyrukta zaten yoksa, sýranýn en baþýna tekrar ekle.
            if (!animalQueue.Contains(selectedAnimal))
            {
                animalQueue.Insert(0, selectedAnimal);
            }
        }

        // Hatalý yerleþtirmeden sonra hayvanýn kurallarýný tekrar göster.
        selectedAnimal.DisplayMyRules();
    }

    private void ShowEffectArea(AnimalData animalData, SeatController potentialSeat)
    {
        // Önce varsa eski highlight'larý temizle.
        ResetAllHighlights();

        // 1. Etki Alaný Menzilini Belirle
        int effectRange = 0;
        if (animalData.turu == AnimalType.Yirtici) effectRange = 1;
        if (animalData.turu == AnimalType.Otobur) effectRange = 1; // Sadece yanýndaki yýrtýcýyý etkiler
        if (animalData.turu == AnimalType.Savunmaci) effectRange = 1;
        // Diðer hayvan türleri için de menziller belirlenebilir...

        if (effectRange > 0)
        {
            // 2. Potansiyel koltuðun etrafýndaki komþularý al.
            List<SeatController> neighbors = gridSystem.GetNeighbors(potentialSeat, effectRange);

            // 3. Bu komþu koltuklarý renklendir.
            foreach (var neighbor in neighbors)
            {
                neighbor.Highlight(effectAreaMaterial);
                currentlyHighlightedSeats.Add(neighbor);
            }
        }
    }

    // Tüm renklendirilmiþ koltuklarý orijinal rengine döndürür.
    private void ResetAllHighlights()
    {
        foreach (var seat in currentlyHighlightedSeats)
        {
            if (seat != null) seat.ResetHighlight();
        }
        currentlyHighlightedSeats.Clear();
    }

}
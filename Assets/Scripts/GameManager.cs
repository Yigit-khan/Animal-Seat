using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using System.Linq;
using Unity.VisualScripting;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;







#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public struct RuleIconData
{
    [Tooltip("Hangi hayvan karakteristiği (trait) için olduğu.")]
    public AnimalTraitSO trait;
    [Tooltip("Bu karakteristiği temsil edecek ikon.")]
    public Sprite icon;
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Seviye Verileri")]
    [SerializeField] private List<GameObject> currentLevelAnimals;

    [Header("Sahne ve Prefab Referanslarý")]
    [SerializeField] private LayerMask seatLayer;
    [SerializeField] private LayerMask animalLayer;

    [Header("Grid Sistemi Ayarlarý")]
    [SerializeField] private Transform seatParent;
    [SerializeField] private Transform gridOriginReference;
    [SerializeField] private Vector2 gridCellSize = new Vector2(1.2f, 1.2f);
    [SerializeField] private bool activateSeatHighlight;


    [Header("Fiziksel Kuyruk ve Sürükleme Ayarlarý")]
    [SerializeField] private Transform[] queuePositions;
    [SerializeField] private float dragLiftHeight = 1.5f;
    [SerializeField] private float queueMoveSpeed = 8f;
    [SerializeField] private float seatHeightOffset = 0.1f;
    [SerializeField] private float dragZOffset = -2f;
    [SerializeField] private Vector3 downwardRayOffset = new Vector3(0, 0, 0.5f);

    [Header("Düşünce Balonu Ayarları")]
    [SerializeField] private GameObject thoughtBubblePrefab;
    // LİSTENİN TÜRÜNÜ DEĞİŞTİRİN:
    [SerializeField] private List<RuleIconData> allRuleIcons;
    // BU DEĞİŞKENİ PUBLİC YAPIN:
    public Vector3 bubbleOffset = new Vector3(0, 1.5f, 0);


    [Header("Can Sistemi Ayarlarý")]
    [Tooltip("Oyuncunun baþlangýçtaki can sayýsý.")]
    private int maxLives = 2;
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
    [SerializeField] private Material greenEffectAreaMaterial;
    [SerializeField] private Material redEffectAreaMaterial;
    [SerializeField] private Material yellowEffectAreaMaterial;
    [SerializeField] private Material whiteEffectAreaMaterial;

    [Header("UI")]
    [SerializeField] private GameObject winUI;
    [SerializeField] private InGameUIManager inGameUIManager;

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
    private AnimalManager _animalManager;
    private List<AnimalSO> animalSOs;

    private Transform startParentOfSelectedAnimal;

    private SeatController lastValidSeatTarget = null;
    private HoldingSlotController lastValidHoldingSlotTarget = null; // YENÝ
    private CoinManager _coinManager;

    //Kazanma ve kaybetme durumu kontrolü
    private bool isWinSequenceStarted = false;
    private bool isGameOverSequenceStarted = false;

    private bool isRecallModeActive = false;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        // 1. Sistemleri kur
        gridSystem = new GridSystem(seatParent, gridOriginReference, gridCellSize);
        _animalManager = new AnimalManager();
        animalSOs = new List<AnimalSO>(); // Kural sisteminin kullanacağı listeyi başlat
        _coinManager = CoinManager.Instance;
        
        isWinSequenceStarted = false;
        isGameOverSequenceStarted = false;

        // 2. Editörde atanan başlangıç hayvanlarını sahneye yerleştir
        PlaceStartingAnimals();

        // 3. Diğer sistemleri kurmaya devam et
        SetupHoldingSlots();
        SetupLives();
        InitializeAnimalQueue();
        SetupAnimalSOs(); // Kuyruktaki hayvanların SO'larını ayarla
    }


    private void Update()
    {
        UpdateAnimalQueuePositions();
        HandlePlayerInput();
    }

    #region Public Fonksiyonlar

    public Sprite GetIconForTrait(AnimalTraitSO traitToFind)
    {
        foreach (var rule in allRuleIcons)
        {
            if (rule.trait == traitToFind)
            {
                return rule.icon;
            }
        }
        // Eğer eşleşen bir ikon bulunamazsa, uyarı ver ve null döndür.
        Debug.LogWarning($"'{traitToFind.name}' için bir ikon bulunamadı. GameManager'daki 'All Rule Icons' listesini kontrol edin.");
        return null;
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

    public void AddOneLife()
    {
        // Eğer canlar zaten maksimumda değilse...
        if (currentLives < maxLives)
        {
            currentLives++;

            // UI'da yeni bir kalp ikonu oluştur.
            if (heartIconPrefab != null && heartsContainer != null)
            {
                GameObject heart = Instantiate(heartIconPrefab, heartsContainer);
                heartIcons.Add(heart);
            }

            Debug.Log("1 can eklendi. Mevcut can: " + currentLives);
        }
    }

    public void ResetLevelForContinue()
    {
        // ... kazanma kaybetme durumu kontrolü.
        isWinSequenceStarted = false; // Seviye sıfırlandığında da sıfırla
        isGameOverSequenceStarted = false;
    }

    public void RestartCurrentLevel()
    {
        // Oyunu durdurmuş olabilecek herhangi bir durumu normale döndür.
        Time.timeScale = 1f;

        // Aktif olan sahnenin adını al ve yeniden yükle.
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);

        Debug.Log(currentSceneName + " sahnesi yeniden başlatılıyor...");
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

    private void SetupAnimalSOs()
    {
        // BU FONKSİYON ARTIK SADECE KUYRUKTAKİ HAYVANLAR İÇİN ÇALIŞIR.
        // Başlangıç hayvanlarının SO'ları PlaceStartingAnimals içinde zaten ayarlandı.
        foreach (var ac in animalQueue)
        {
            if (ac.animalSO == null)
                continue;

            AnimalSO runtimeSO = ScriptableObject.Instantiate(ac.animalSO);
            runtimeSO.gridOriginPos = new Vector2Int(-1, -1); // Kuyruktaki hayvanların pozisyonu yoktur
            ac.animalSO = runtimeSO;
            animalSOs.Add(runtimeSO);
        }
    }
    private void InitializeAnimalQueue()
    {
        animalQueue.Clear();
        // currentLevelAnimals listesinden kuyruğa hayvan ekle.
        // Not: Seviye başında yerleştirdiğiniz hayvanları bu listeden çıkarabilirsiniz,
        // böylece aynı hayvanlar hem oturup hem de kuyrukta olmaz.
        int count = Mathf.Min(currentLevelAnimals.Count, queuePositions.Length);
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = currentLevelAnimals[i];
            if (prefab == null) continue;

            var animalController = Instantiate(
                prefab,
                queuePositions[i].position,
                Quaternion.identity
            ).GetComponent<AnimalController>();

            if (animalController != null)
            {
                animalController.Initialize();

                // --- İŞTE YENİ EKLENEN SATIR ---
                // Sadece kuyruğa eklenen hayvanların kurallarını/balonlarını göster.
                animalController.DisplayMyRules();

                animalQueue.Add(animalController);
            }
        }
    }
    private void UpdateAnimalQueuePositions()
    {
        int maxIndex = Mathf.Min(animalQueue.Count, queuePositions.Length);

        for (int i = 0; i < maxIndex; i++)
        {
            if (animalQueue[i] == selectedAnimal) continue;

            animalQueue[i].transform.position = Vector3.Lerp(
                animalQueue[i].transform.position,
                queuePositions[i].position,
                Time.deltaTime * queueMoveSpeed
            );
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

        if (isRecallModeActive)
        {
            HandleRecallClick();
            return;
        }

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

                    SoundManager.Instance.PlaySFX("AnimalGrab");

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

                SoundManager.Instance.PlaySFX("AnimalGrab");

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
        Ray downwardRay = new Ray(selectedAnimal.transform.position + downwardRayOffset, Vector3.down);
        bool foundTarget = false;
        Debug.DrawRay(downwardRay.origin, downwardRay.direction * 50, Color.green);

        // Önce, hayvanýn altýnda bir ANA KOLTUK var mý?
        if (Physics.Raycast(downwardRay, out RaycastHit hit, 20f, seatLayer))
        {

            if (hit.collider.TryGetComponent<SeatController>(out SeatController targetSeat))
            {
                foundTarget = true;
                lastValidHoldingSlotTarget = null; // Diðer hedefi temizle
                if (lastValidSeatTarget != targetSeat)
                {
                    
                    ShowEffectArea(selectedAnimal.animalSO, targetSeat);
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
            selectedAnimal.animalSO.gridOriginPos = new Vector2Int(-1, -1);
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
        bool isValid = IsPlacementValid(targetSeat);
        if (!isValid)
        {
            SoundManager.Instance.PlaySFX("PlacementWrong");

            LoseLife();
            return false; // Yerleþtirme baþarýsýz.
        }

        SoundManager.Instance.PlaySFX("PlacementCorrect");

        // Kurallar uygunsa, hayvaný bu hedefe yerleþtir.
        PlaceAnimalOnSeat(selectedAnimal, targetSeat);

        return true; // Yerleþtirme baþarýlý.
    }

    private void PlaceAnimalOnSeat(AnimalController animal, SeatController mainSeat)
    {
        animal.isSeated = true;
        animal.ClearMyBubbles();
        animal.transform.position = mainSeat.transform.position + new Vector3(0, seatHeightOffset, 0);

        // --- YENİ EKLENEN KISIM ---
        // Hayvanın hangi koltukları işgal ettiğini listesine kaydet.
        animal.occupiedSeats.Clear();
        Vector2Int size = animal.animalSO.size;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int pos = new Vector2Int(mainSeat.GridPosition.x + x, mainSeat.GridPosition.y + y);
                SeatController currentSeat = gridSystem.GetSeatAt(pos);
                if (currentSeat != null)
                {
                    currentSeat.Occupy(animal);
                    animal.occupiedSeats.Add(currentSeat); // Listeye ekle
                }
            }
        }
        // --- BİTİŞ ---

        animalQueue.Remove(animal);
        animal.gameObject.layer = animal.originalLayer;
        CheckWinCondition();
    }

    private bool IsPlacementValid(SeatController targetSeat)
    {
        if (selectedAnimal == null)
        {
            Debug.LogError("IsPlacementValid çağrıldı ancak selectedAnimal null!");
            return false;
        }

        // 1) CurrentLevelAnimals içindeki prefab'lar değil,
        //    runtime'daki AnimalController'lar üzerinden veri toplayın.
        //    Aksi taktirde prefab.GetComponent<AnimalController>() null döner.


        // 2) Önce koltuğu alıp null kontrolü yapın!
        Vector2Int checkPos = targetSeat.GridPosition;
        SeatController adjacentSeat = gridSystem.GetSeatAt(checkPos);
        if (adjacentSeat == null)
        {
            Debug.LogWarning($"Soldaki komşu koltuk bulunamadı: [{checkPos.x},{checkPos.y}]");
            return false;
        }

        // 3) Burada SeatController.GridPosition’ı doğrudan değiştirmek yerine
        //    yeni bir Vector2Int ile gridOriginPos’u hesaplayın:
        Vector2Int newOrigin = new Vector2Int(
            adjacentSeat.GridPosition.x,
            adjacentSeat.GridPosition.y
        );

        // 4) animalSO’nun null olmadığından emin olun
        var animalController = selectedAnimal;
        if (animalController.animalSO == null)
        {
            Debug.LogError($"{animalController.name} üzerinde AnimalSO yok!");
            return false;
        }

        animalController.animalSO.gridOriginPos = newOrigin;
        //Debug.Log($"{animalController.animalSO._animalName} yeni gridOriginPos: {newOrigin}");

        // 5) Interaction testi için doğru listeyi kullanın
        bool isValid = _animalManager.IsAllInteractionsValid(animalSOs);
        //Debug.Log("isValid: " + isValid);
        return isValid;
    }

    private void LoseLife()
    {
        Debug.Log("LoseLife ÇAĞRILDI. Mevcut Can: " + (currentLives - 1)); 
        if (currentLives <= 0) return;
        currentLives--;
        if (heartIcons.Count > 0)
        {
            Destroy(heartIcons[heartIcons.Count - 1]);
            heartIcons.RemoveAt(heartIcons.Count - 1);
        }
        if (currentLives <= 0) GameOver(false);
    }

    // GameManager.cs

    private void GameOver(bool isSoftLock) // DEĞİŞTİ: Parametre eklendi.
    {
        if (isGameOverSequenceStarted) return;
        isGameOverSequenceStarted = true;

        // Neye göre kaybedildiğine bağlı olarak metni belirle.
        string loseReasonText = isSoftLock ? "NO MOVES LEFT" : "FAILED"; // YENİ

        float loseDelay = 0.5f;
        DOVirtual.DelayedCall(loseDelay, () =>
        {
            SoundManager.Instance.PlaySFX("LevelFail");

            if (inGameUIManager != null)
            {
                // Belirlediğimiz metni UI yöneticisine gönder.
                inGameUIManager.ShowLoseUI(loseReasonText); // DEĞİŞTİ
            }
            else
            {
                Debug.LogError("GameManager'daki 'In Game UI Manager' referansı atanmamış!");
            }
        }).SetUpdate(true);
    }

    private void CheckWinCondition()
    {
        // Eğer kazanma süreci zaten başlamışsa, tekrar kontrol etme.
        if (isWinSequenceStarted) return;

        bool isHoldingSlotsOccipied = false;
        foreach (var slot in holdingSlots)
        {
            if (slot.CurrentState == SlotState.Occupied)
            {
                isHoldingSlotsOccipied = true;
                break;
            }
        }

        // Kazanma koşulu sağlandı mı?
        if (animalQueue.Count == 0 && !isHoldingSlotsOccipied)
        {
            // Kazanma sürecini başlat ve tekrar başlatılmasını engelle.
            isWinSequenceStarted = true;

            // --- DOTWEEN GECİKMESİ BURADA ---
            float winDelay = 0.5f; // 0.5 saniye gecikme
            DOVirtual.DelayedCall(winDelay, () =>
            {
                // Bu kod, 0.5 saniye sonra çalışacak.
                SoundManager.Instance.PlaySFX("LevelWin");
                Debug.Log("TEBRİKLER! SEVİYE TAMAMLANDI!");

                if (inGameUIManager != null)
                {
                    inGameUIManager.WinUIAnimation();
                }
                else
                {
                    Debug.LogError("InGameUIManager referansı atanmamış!");
                }
            });
        }
        else
        {
            // Soft-lock kontrolü olduğu gibi kalabilir.
            List<AnimalSO> waitingAnimals = animalQueue.Select(a => a.animalSO).ToList();
            List<AnimalSO> seatedSOs = animalSOs.Where(so => so.gridOriginPos.x >= 0).ToList();
            foreach (var animal in holdingSlots)
                if (animal.OccupyingAnimal != null && animal.OccupyingAnimal.animalSO != null)
                    waitingAnimals.Add(animal.OccupyingAnimal.animalSO);

            bool isSoftLocked = _animalManager.IsSoftLocked(waitingAnimals, gridSystem.GetAllEmptySeats(), seatedSOs);
            if (isSoftLocked)
            {
                Debug.LogWarning("Soft lock BULUNDU! Game over...");
                GameOver(true);
            }
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

        SoundManager.Instance.PlaySFX("PlaceToHoldingSlot");

        // Hayvaný bu hedefe yerleþtir.
        targetSlot.PlaceAnimal(selectedAnimal);
        selectedAnimal.transform.position = targetSlot.transform.position + new Vector3(0, seatHeightOffset, 0);
        animalQueue.Remove(selectedAnimal);
        return true;
    }

    private void ReturnAnimalToOrigin()
    {
        selectedAnimal.isSeated = false; // Artık oturmuyor.
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

    private void ShowEffectArea(AnimalSO animalSO, SeatController potentialSeat)
    {
        // Önce varsa eski highlight'larý temizle.
        ResetAllHighlights();

        List<SeatController> neighbors = new List<SeatController>();
        foreach (var trait in animalSO.traits)
        {
            var effectPoints = trait.GetTraitEffectPoints(potentialSeat.GridPosition, animalSO.size);
            //print(string.Join(", ", effectPoints));
            foreach (var point in effectPoints)
            {
                if (point != null)
                {
                    var seat = gridSystem.GetSeatAt(point);
                    if (seat != null)
                        neighbors.Add(seat);
                }
            }
        }

        if (!activateSeatHighlight)
        {
            if (!potentialSeat.isOccupied)
            {
                potentialSeat.Highlight(yellowEffectAreaMaterial);
                currentlyHighlightedSeats.Add(potentialSeat);
                foreach (var neighbor in neighbors)
                {
                    neighbor.Highlight(whiteEffectAreaMaterial);
                    currentlyHighlightedSeats.Add(neighbor);
                }
            }
            return;
        }

        selectedAnimal.animalSO.gridOriginPos = potentialSeat.GridPosition;
        if (!_animalManager.IsAllInteractionsValid(animalSOs))
        {
            potentialSeat.Highlight(redEffectAreaMaterial);
            currentlyHighlightedSeats.Add(potentialSeat);
            return;
        }

        potentialSeat.Highlight(greenEffectAreaMaterial);
        currentlyHighlightedSeats.Add(potentialSeat);

        foreach (var neighbor in neighbors)
        {
            neighbor.Highlight(greenEffectAreaMaterial);
            currentlyHighlightedSeats.Add(neighbor);
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

    private void PlaceStartingAnimals()
    {
        // Grid sistemindeki tüm koltukları al
        Dictionary<Vector2Int, SeatController> allSeats = gridSystem.GetFullGrid();
        foreach (var seatPair in allSeats)
        {
            SeatController seat = seatPair.Value;
            if (seat.startingAnimalPrefab != null && !seat.isOccupied)
            {
                GameObject animalObj = Instantiate(
                    seat.startingAnimalPrefab,
                    seat.transform.position + new Vector3(0, seatHeightOffset, 0),
                    Quaternion.identity
                );
                if (animalObj.TryGetComponent<AnimalController>(out var animalController))
                {
                    // Bu fonksiyon artık balon OLUŞTURMUYOR, bu yüzden burada çağırmak güvenli.
                    animalController.Initialize();

                    animalController.isSeated = true;

                    // 3. Kural sisteminin kullanması için AnimalSO'dan bir RUNTIME KOPYASI oluştur
                    AnimalSO runtimeSO = ScriptableObject.Instantiate(animalController.animalSO);
                    runtimeSO.gridOriginPos = seat.GridPosition; // Grid pozisyonunu bu kopyaya ata
                    animalController.animalSO = runtimeSO; // Controller'ın artık bu kopyayı kullanmasını sağla
                    animalSOs.Add(runtimeSO); // Hayvanı, kural yöneticisinin listesine ekle

                    // 4. Hayvanın boyutuna göre kapladığı TÜM koltukları "dolu" olarak işaretle
                    Vector2Int size = runtimeSO.size;
                    for (int x = 0; x < size.x; x++)
                    {
                        for (int y = 0; y < size.y; y++)
                        {
                            // Not: Grid'inizin Y ekseninin nasıl çalıştığına göre (aşağı mı yukarı mı artıyor)
                            // buradaki 'seat.GridPosition.y + y' ifadesini '- y' olarak değiştirmeniz gerekebilir.
                            // Genellikle +y doğrudur.
                            Vector2Int currentPos = new Vector2Int(seat.GridPosition.x + x, seat.GridPosition.y + y);
                            SeatController occupiedSeat = gridSystem.GetSeatAt(currentPos);
                            if (occupiedSeat != null)
                            {
                                occupiedSeat.Occupy(animalController);
                            }
                        }
                    }
                    Debug.Log($"Başlangıç hayvanı '{runtimeSO._animalName}', grid pozisyonu [{seat.GridPosition.x},{seat.GridPosition.y}]'e yerleştirildi.");
                }
                else
                {
                    Debug.LogError($"{seat.startingAnimalPrefab.name} prefab'ında AnimalController componenti bulunamadı! Hayvan yerleştirilemedi.");
                    Destroy(animalObj);
                }
            }
        }
    }

    #region Power-Up Fonksiyonları

    /// <summary>
    /// Power-up butonuna basıldığında çağrılır ve geri alma modunu başlatır.
    /// </summary>
    public void ActivateRecallMode()
    {
        // Eğer seçili bir hayvan varsa veya power-up yoksa modu aktif etme.
        if (selectedAnimal != null || PowerUpController.Instance.GetPowerUpCount() <= 0)
        {
            SoundManager.Instance.PlaySFX("RecallFail"); // Hata sesi çal
            return;
        }

        isRecallModeActive = true;
        PowerUpController.Instance.SetRecallModeActiveVisuals(true); // UI'ı güncelle
        StartShakingSeatedAnimals(); 
        SoundManager.Instance.PlaySFX("PowerUpActivate"); // Mod aktif sesi çal
        Debug.Log("Geri Alma Modu Aktif. Geri alınacak hayvanı seçin.");
    }

    /// <summary>
    /// Geri alma modunu iptal eder.
    /// </summary>
    private void DeactivateRecallMode()
    {
        isRecallModeActive = false;
        PowerUpController.Instance.SetRecallModeActiveVisuals(false);
        StopShakingSeatedAnimals();
    }

    /// <summary>
    /// Geri alma modu aktifken yapılan tıklamaları yönetir.
    /// </summary>
    private void HandleRecallClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, animalLayer))
        {
            // Tıklanan objenin bir hayvan olup olmadığını ve oturup oturmadığını kontrol et.
            if (hit.collider.TryGetComponent<AnimalController>(out var animal) && animal.isSeated)
            {
                // Başarılı! Hayvanı geri çağır.
                RecallAnimal(animal);
            }
            else
            {
                // Oturmayan bir hayvana veya boş bir yere tıklandıysa modu iptal et.
                DeactivateRecallMode();
            }
        }
        else
        {
            // Herhangi bir şeye tıklanmadıysa modu iptal et.
            DeactivateRecallMode();
        }
    }

    /// <summary>
    /// Belirtilen hayvanı oturduğu yerden kaldırıp kuyruğun başına ekler.
    /// </summary>
    private void RecallAnimal(AnimalController animalToRecall)
    {
        animalToRecall.transform.DOKill();
        animalToRecall.transform.rotation = Quaternion.identity;

        // 1. Power-up'ı kullan ve sayıyı düşür.
        PowerUpController.Instance.UsePowerUp();

        // 2. Hayvanın oturduğu tüm koltukları boşalt.
        foreach (var seat in animalToRecall.occupiedSeats)
        {
            if (seat != null) seat.Vacate();
        }
        animalToRecall.occupiedSeats.Clear();

        // 3. Hayvanın durumunu güncelle.
        animalToRecall.isSeated = false;
        animalToRecall.animalSO.gridOriginPos = new Vector2Int(-1, -1);

        // 4. Hayvanı kuyruğun en başına ekle.
        animalQueue.Insert(0, animalToRecall);

        // 5. Kurallarını tekrar göster.
        animalToRecall.DisplayMyRules();

        // 6. Geri alma modunu kapat.
        DeactivateRecallMode();

        // 7. Başarı sesi çal.
        SoundManager.Instance.PlaySFX("RecallSuccess");
        Debug.Log($"{animalToRecall.animalSO._animalName} geri çağrıldı!");
    }

    private void StartShakingSeatedAnimals()
    {
        // animalSOs listesi, hem oturan hem de kuyruktaki hayvanları içerir.
        // Sadece oturanları (grid pozisyonu olanları) filtrele.
        foreach (var so in animalSOs)
        {
            if (so != null && so.gridOriginPos.x != -1) // gridOriginPos kontrolü yerine isSeated de kullanılabilir
            {
                // Hayvanın controller'ını bulmamız lazım.
                // animalSOs listesi SO'ları tuttuğu için, controller'ı bulmak için sahneyi taramalıyız.
                // DAHA İYİ YÖNTEM: Hayvanların kendilerini bir listeye kaydetmesini sağlamak.
                // Ama şimdilik basit bir çözümle ilerleyelim.
            }
        }

        // Yukarıdaki yöntem karmaşık. DAHA BASİT VE GÜVENİLİR YÖNTEM:
        // Sahnede AnimalController component'ine sahip tüm objeleri bul.
        AnimalController[] allAnimalsOnScene = FindObjectsOfType<AnimalController>();
        foreach (var animal in allAnimalsOnScene)
        {
            if (animal.isSeated)
            {
                // DOTween'in PunchRotation'ı mükemmel bir titreme efekti verir.
                // "animal" objesinin transform'una bir tween ID'si ("shake") atıyoruz ki daha sonra durdurabilelim.
                animal.transform.DOPunchRotation(new Vector3(0, 0, 5f), 1f, 10, 1)
                    .SetLoops(-1, LoopType.Restart) // Sonsuz döngü
                    .SetId("shake");
            }
        }
    }

    /// <summary>
    /// Hayvanlardaki tüm titreme animasyonlarını durdurur.
    /// </summary>
    private void StopShakingSeatedAnimals()
    {
        // "shake" ID'sine sahip tüm DOTween animasyonlarını durdur.
        DOTween.Kill("shake");

        // Her ihtimale karşı hayvanların rotasyonunu sıfırla.
        AnimalController[] allAnimalsOnScene = FindObjectsOfType<AnimalController>();
        foreach (var animal in allAnimalsOnScene)
        {
            if (animal.isSeated)
            {
                animal.transform.DORotate(Vector3.zero, 0.1f);
            }
        }
    }

    #endregion
}
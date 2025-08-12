using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using System.Linq;
using Unity.VisualScripting;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Interactions;

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

//Akif : Oyun durumu kontrolü, lose ekranı geldiğinde hayvanlar hareket ettirilemesin diye oyunun durumunu kontrol için ekledim
public enum GameState { Playing, Paused, Won, Lost }

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
    [SerializeField] private Vector3 eyepatchOffset = new Vector3(0f, -0.5f, -0.25f);
    [SerializeField] private float dragSwayAmount = 5f;    // Sürüklerken ne kadar yana eğileceği (derece).
    [SerializeField] private float dragSwayDuration = 1f; // Bir tam sallanma döngüsünün süresi.

    [Header("Düşünce Balonu Ayarları")]
    [SerializeField] private GameObject thoughtBubblePrefab;
    public Vector3 bubbleOffset = new Vector3(0, 1.5f, 0);


    [Header("Can Sistemi Ayarlarý")]
    [Tooltip("Oyuncunun baþlangýçtaki can sayýsý.")]
    private int maxLives = 2;
    [Tooltip("Can ikonlarýnýn oluþturulacaðý UI parent'ý.")]
    [SerializeField] private Transform heartsContainer;
    [Tooltip("Bir caný temsil eden UI prefab'ý.")]
    [SerializeField] private GameObject heartIconPrefab;

    [Header("Bekleme Koltuðu Ayarlarý")]
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
    [SerializeField] public Material greenEffectAreaMaterial;
    [SerializeField] private Material redEffectAreaMaterial;
    [SerializeField] private Material yellowEffectAreaMaterial;
    [SerializeField] private Material whiteEffectAreaMaterial;

    [Header("UI")]
    [SerializeField] private GameObject winUI;
    [SerializeField] private InGameUIManager inGameUIManager;


    // --- Özel Deðiþkenler ---
    private List<SeatController> currentlyHighlightedSeats = new List<SeatController>();
    private List<HoldingSlotController> currentlyHighlightedHoldingSlots = new List<HoldingSlotController>();

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
    private PowerupSO _recallPowerUpSO;
    private PowerupSO _eyepatchPowerUpSO;


    //Kazanma ve kaybetme durumu kontrolü
    private bool isWinSequenceStarted = false;
    private bool isGameOverSequenceStarted = false;

    private bool isRecallModeActive = false;
    private bool isEyepatchModeActive = false;

    public static GameState CurrentGameState { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        dragZOffset = 0.6f;

        Time.timeScale = 1f;

        gridSystem = new GridSystem(seatParent, gridOriginReference, gridCellSize);
        _animalManager = new AnimalManager();
        animalSOs = new List<AnimalSO>(); // Kural sisteminin kullanacağı listeyi başlat
        _coinManager = CoinManager.Instance;

        CurrentGameState = GameState.Playing;
        Time.timeScale = 1f;

        isWinSequenceStarted = false;
        isGameOverSequenceStarted = false;

        PlaceStartingAnimals();

        SetupHoldingSlots();
        SetupLives();
        InitializeAnimalQueue();
        SetupAnimalSOs(); // Kuyruktaki hayvanların SO'larını ayarla
        SetupPowerupSOs();
    }


    private void Update()
    {
        UpdateAnimalQueuePositions();
        HandlePlayerInput();
    }

    #region Public Fonksiyonlar

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
        if (currentLives < maxLives)
        {
            currentLives++;

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
        isWinSequenceStarted = false; 
        isGameOverSequenceStarted = false;
    }

    public void RestartCurrentLevel()
    {
        CurrentGameState = GameState.Playing;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PauseGame()
    {
        if (CurrentGameState == GameState.Playing)
        {
            CurrentGameState = GameState.Paused;
            Time.timeScale = 0f;
            Debug.Log("Oyun Duraklatıldı.");
        }
    }

    public void ResumeGame()
    {
        if (CurrentGameState == GameState.Paused)
        {
            CurrentGameState = GameState.Playing;
            Time.timeScale = 1f;
            Debug.Log("Oyun Devam Ediyor.");
        }
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
            runtimeSO.gridOriginPos = new Vector2Int(-1, -1); // grid dışında
            ac.animalSO = runtimeSO;
            animalSOs.Add(runtimeSO);
        }
    }

    private void SetupPowerupSOs()
    {
        _recallPowerUpSO = ScriptableObject.Instantiate(PowerUpController.Instance.GetReferenceByName("Recall").so);
        _eyepatchPowerUpSO = ScriptableObject.Instantiate(PowerUpController.Instance.GetReferenceByName("Eyepatch").so);

    }
    private void InitializeAnimalQueue()
    {
        animalQueue.Clear();
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
        if (CurrentGameState != GameState.Playing)
        {
            return;
        }

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
        else if (isEyepatchModeActive)
        {
            HandleEyepatchClick();
            return;
        }

        if (selectedAnimal != null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

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
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(mouseRay, out float enter))
        {
            var p = mouseRay.GetPoint(enter) + offset;
            p.y = dragLiftHeight;
            selectedAnimal.transform.position = p;
        }

        bool foundTarget = false;

        Vector3 boxOffset = new Vector3(0, -0.2f, 0.5f);
        Vector3 halfExtents = new Vector3(0.4f, 4f, 0.5f);
        Vector3 boxCenter = selectedAnimal.transform.position
                              + boxOffset
                              - Vector3.up * halfExtents.y;
        Quaternion boxRot = Quaternion.identity;

        // iki kere overlapbox yapmak yerine layerları birleştiriyoruz
        LayerMask allSeats = seatLayer | holdingSlotLayer;

        // ana koltuklar
        var hits = Physics.OverlapBox(boxCenter, halfExtents, boxRot, allSeats);


        if (hits.Length > 0 &&
            hits[0].TryGetComponent<SeatController>(out var targetSeat))
        {
            foundTarget = true;
            lastValidHoldingSlotTarget = null;

            if (lastValidSeatTarget != targetSeat)
            {
                ShowEffectArea(selectedAnimal.animalSO, targetSeat);
                lastValidSeatTarget = targetSeat;
            }
        }
        else
        {
            // bekleme slotları
            foreach (var col in hits)
            {
                if (col.TryGetComponent<HoldingSlotController>(out var slot) &&
                    slot.CurrentState == SlotState.Unlocked)
                {
                    foundTarget = true;

                    // Önce tüm eski highlight’ları temizle
                    ResetAllHighlights();
                    lastValidSeatTarget = null;
                    lastValidHoldingSlotTarget = slot;

                    // ► Holding slot’a highlight uygula ◄
                    slot.Highlight(greenEffectAreaMaterial);
                    currentlyHighlightedHoldingSlots.Add(slot);

                    break;
                }
            }
        }

        if (!foundTarget)
        {
            ResetAllHighlights();
            lastValidSeatTarget = null;
            lastValidHoldingSlotTarget = null;
        }

        DebugDrawBox(boxCenter, halfExtents, boxRot, foundTarget ? Color.green : Color.red);
    }

    private void DebugDrawBox(Vector3 center, Vector3 halfExtents, Quaternion rot, Color c)
    {
        // 8 köşeyi hesaplayıp çiziyoruz
        Vector3[] points = new Vector3[8];
        int idx = 0;
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                    points[idx++] = center + rot * Vector3.Scale(halfExtents, new Vector3(x, y, z));

        Debug.DrawLine(points[0], points[1], c);
        Debug.DrawLine(points[2], points[3], c);
        Debug.DrawLine(points[4], points[5], c);
        Debug.DrawLine(points[6], points[7], c);
        Debug.DrawLine(points[0], points[2], c);
        Debug.DrawLine(points[1], points[3], c);
        Debug.DrawLine(points[4], points[6], c);
        Debug.DrawLine(points[5], points[7], c);
        Debug.DrawLine(points[0], points[4], c);
        Debug.DrawLine(points[1], points[5], c);
        Debug.DrawLine(points[2], points[6], c);
        Debug.DrawLine(points[3], points[7], c);
    }



    private void HandleMouseUp()
    {
        if (selectedAnimal == null) return;

        // animasyon sifirla
        DOTween.Kill("drag_sway");
        selectedAnimal.transform.rotation = Quaternion.identity;


        ResetAllHighlights();

        bool placedSuccessfully = false;

        if (lastValidSeatTarget != null)
        {
            placedSuccessfully = TryPlaceOnSeat(lastValidSeatTarget);
        }
        else if (lastValidHoldingSlotTarget != null)
        {
            placedSuccessfully = TryPlaceOnHoldingSlot(lastValidHoldingSlotTarget);
        }

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
        if (targetSeat == null || selectedAnimal == null) return false;

        // --- simülasyon başlangıcı ---
        Vector2Int originalPos = selectedAnimal.animalSO.gridOriginPos;
        selectedAnimal.animalSO.gridOriginPos = targetSeat.GridPosition;

        bool isValid = _animalManager.IsAllInteractionsValid(animalSOs, out List<AnimalSO> affectedAnimals);

        // --- simülasyonu geri Al ---
        selectedAnimal.animalSO.gridOriginPos = originalPos;

        if (!isValid)
        {
            SoundManager.Instance.PlaySFX("PlacementWrong");

            Debug.Log("Hatalı yerleştirme! Etkilenen hayvan(lar):");

            // 2. ÖNCE, o an yerleştirmeye çalıştığımız hayvana efekti uygula.
            // Çünkü bu hamlenin kendisi hatalı.
            selectedAnimal.PlayErrorFeedback();

            // 3. SONRA, bu hamleden rahatsız olan DİĞER hayvanlara da efekti uygula.
            var allControllers = AnimalController.Instances;
            foreach (var controller in allControllers)
            {
                // Eğer bu controller, etkilenenler listesindeyse VE
                // o an seçili olan hayvanın kendisi değilse (çünkü ona zaten uyguladık)...
                if (controller.animalSO != null && affectedAnimals.Contains(controller.animalSO) && controller != selectedAnimal)
                {
                    Debug.Log("- " + controller.animalSO._animalName);
                    controller.PlayErrorFeedback(); // Hata efektini oynat!
                }
            }

            LoseLife();
            return false;
        }

        // hamle geçerliyse...
        SoundManager.Instance.PlaySFX("PlacementCorrect");
        selectedAnimal.animalSO.gridOriginPos = targetSeat.GridPosition;
        PlaceAnimalOnSeat(selectedAnimal, targetSeat);
        return true;
    }

    private void PlaceAnimalOnSeat(AnimalController animal, SeatController mainSeat)
    {
        animal.isSeated = true;
        animal.ClearMyBubbles();
        animal.transform.position = mainSeat.transform.position + new Vector3(0, seatHeightOffset, 0);

        if (animal.TryGetComponent<Animator>(out var animator))
        {
            animator.SetBool("isSeated", true);
        }

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


        if (animal.animalSO.eyepatched && animal.eyepatchRenderer != null)
        {
            var tr = animal.eyepatchRenderer.transform;

            tr.DOKill();

            Vector3 targetPos = tr.position + eyepatchOffset;

            tr.DOMove(targetPos, 0.4f)
              .SetEase(Ease.OutQuad);
        }

        animalQueue.Remove(animal);
        animal.gameObject.layer = animal.originalLayer;
        CheckWinCondition();
    }

    /*
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
    */

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

        if (currentLives <= 0)
        {
            GameOver(false); 
            return;
        }

        List<AnimalSO> waitingAnimals = animalQueue.Select(a => a.animalSO).ToList();
        List<AnimalSO> seatedSOs = animalSOs.Where(so => so.gridOriginPos.x >= 0).ToList();

        foreach (var slot in holdingSlots)
        {
            if (slot.CurrentState == SlotState.Occupied && slot.OccupyingAnimal != null)
            {
                waitingAnimals.Add(slot.OccupyingAnimal.animalSO);
            }
        }

        bool isSoftLocked = _animalManager.IsSoftLocked(waitingAnimals, gridSystem.GetAllEmptySeats(), seatedSOs);
        if (isSoftLocked)
        {
            Debug.LogWarning("CAN KAYBINDAN SONRA Soft lock TESPİT EDİLDİ! Oyun bitiriliyor...");
            GameOver(true); // Soft-lock nedeniyle oyun bitti.
        }
    }

    private void GameOver(bool isSoftLock)
    {
        if (isGameOverSequenceStarted) return;
        isGameOverSequenceStarted = true;


        CurrentGameState = GameState.Lost;

        if (tutorialAnimScript.Instance != null)
        {
            tutorialAnimScript.Instance.ForceClose();
        }

        string loseReasonText = isSoftLock ? "NO MOVES LEFT" : "FAILED"; // YENİ

        float loseDelay = 0.5f;
        DOVirtual.DelayedCall(loseDelay, () =>
        {
            SoundManager.Instance.PlaySFX("LevelFail");

            if (inGameUIManager != null)
            {
                inGameUIManager.ShowLoseUI(loseReasonText);
            }
            else
            {
                Debug.LogError("GameManager'daki 'In Game UI Manager' referansı atanmamış!");
            }
        }).SetUpdate(true);
    }

    private void CheckWinCondition()
    {
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

        if (animalQueue.Count == 0 && !isHoldingSlotsOccipied)
        {
            isWinSequenceStarted = true;

            CurrentGameState = GameState.Won;

            float winDelay = 0.2f;
            DOVirtual.DelayedCall(winDelay, () =>
            {

                int currentLevel = SaveManager.LoadCurrentLevel();
                int unlockedLevel = SaveManager.LoadLevel();
                if (currentLevel >= unlockedLevel)
                {
                    SaveManager.SaveLevel(currentLevel + 1);
                    Debug.Log($"SEVİYE {currentLevel + 1} KİLİDİ AÇILDI!");
                }

                int levelCoinReward = 100; // Örnek bir ödül miktarı //sonradan levela göre al
                if (_coinManager != null)
                {
                    _coinManager.AddCoins(levelCoinReward);
                    Debug.Log($"{levelCoinReward} COIN KAZANILDI VE KAYDEDİLDİ! Toplam: {_coinManager.CurrentCoins}");
                }

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
                if (tutorialAnimScript.Instance != null)
                {
                    tutorialAnimScript.Instance.ForceClose();
                }
            });
        }
        else
        {
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
        foreach (Transform child in holdingSlotsParent)
        {
            Destroy(child.gameObject);
        }
        holdingSlots.Clear();

        for (int i = 0; i < totalHoldingSlots; i++)
        {
            Vector3 position = holdingSlotsParent.position + new Vector3(i * holdingSlotSpacing, 0, 0);

            if (i >= unlockedSlotsAtStart)
            {
                GameObject slotObj = Instantiate(lockedSlotPrefab, position, Quaternion.identity, holdingSlotsParent);
                HoldingSlotController controller = slotObj.GetComponent<HoldingSlotController>();

                if (controller != null)
                {
                    controller.Initialize(SlotState.Locked);
                    holdingSlots.Add(controller);

                }
            }
            else
            {
                GameObject slotObj = Instantiate(holdingSlotPrefab, position, Quaternion.identity, holdingSlotsParent);
                slotObj.transform.rotation = holdingSlotsParent.rotation;
                HoldingSlotController controller = slotObj.GetComponent<HoldingSlotController>();
                if (controller != null)
                {
                    controller.Initialize(SlotState.Unlocked);
                    holdingSlots.Add(controller);
                    if (holdingSlotsParent.tag == "Tutorial")
                        controller.TutorialScaleAnim();
                }
            }
        }
    }

    private void StartDraggingSelectedAnimal()
    {
        if (selectedAnimal == null) return;

        selectedAnimal.ClearMyBubbles();
        selectedAnimal.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // sallama animasyonu
        selectedAnimal.transform.DORotate(new Vector3(0, 0, dragSwayAmount), dragSwayDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetId("drag_sway");

        dragPlane = new Plane(Vector3.up, new Vector3(0, dragLiftHeight, 0));

        offset = new Vector3(0f, 0f, dragZOffset);
    }

    private void TryUnlockSlot(HoldingSlotController lockedSlot)
    {
        Debug.Log("Kilitli slota týklandý! Kilit açýlýyor...");

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
        if (targetSlot == null || targetSlot.CurrentState != SlotState.Unlocked)
        {
            return false;
        }

        SoundManager.Instance.PlaySFX("PlaceToHoldingSlot");

        targetSlot.PlaceAnimal(selectedAnimal);
        selectedAnimal.transform.position = targetSlot.transform.position + new Vector3(0, seatHeightOffset, 0);
        animalQueue.Remove(selectedAnimal);
        return true;
    }

    private void ReturnAnimalToOrigin()
    {
        selectedAnimal.isSeated = false;
        if (startParentOfSelectedAnimal != null && startParentOfSelectedAnimal.TryGetComponent<HoldingSlotController>(out var slot))
        {
            slot.PlaceAnimal(selectedAnimal);
            selectedAnimal.transform.position = slot.transform.position + new Vector3(0, seatHeightOffset, 0);
        }
        else
        {
            if (!animalQueue.Contains(selectedAnimal))
            {
                animalQueue.Insert(0, selectedAnimal);
            }
        }

        selectedAnimal.DisplayMyRules();
    }

    private void ShowEffectArea(AnimalSO animalSO, SeatController potentialSeat)
    {
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

    private void ResetAllHighlights()
    {
        foreach (var seat in currentlyHighlightedSeats)
            seat?.ResetHighlight();
        currentlyHighlightedSeats.Clear();

        foreach (var slot in currentlyHighlightedHoldingSlots)
            slot?.ResetHighlight();
        currentlyHighlightedHoldingSlots.Clear();
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
                    animalController.Initialize();

                    animalController.isSeated = true;
                    animalController.isRecallable = false;
                    animalController.occupiedSeats.Add(seat);

                    AnimalSO runtimeSO = ScriptableObject.Instantiate(animalController.animalSO);
                    runtimeSO.gridOriginPos = seat.GridPosition;
                    animalController.animalSO = runtimeSO;
                    animalSOs.Add(runtimeSO);

                    Vector2Int size = runtimeSO.size;
                    for (int x = 0; x < size.x; x++)
                    {
                        for (int y = 0; y < size.y; y++)
                        {
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
        // Eğer zaten seçili bir hayvan varsa veya hakkınız kalmadıysa başarısız
        if (selectedAnimal != null || _recallPowerUpSO.RemainingUse <= 0)
        {
            SoundManager.Instance.PlaySFX("RecallFail");
            return;
        }

        isRecallModeActive = true;
        PowerUpController.Instance.SetPowerUpVisuals(_recallPowerUpSO, true);
        StartShakingSeatedAnimals(AnimalController.Instances.Where(animal => animal.isSeated && animal.isRecallable));
        SoundManager.Instance.PlaySFX("PowerUpActivate");
        Debug.Log($"{_recallPowerUpSO.powerupName} modu aktif. Geri alınacak hayvanı seçin.");
    }

    /// <summary>
    /// Geri alma modunu iptal eder.
    /// </summary>
    private void DeactivateRecallMode()
    {
        isRecallModeActive = false;
        PowerUpController.Instance.SetPowerUpVisuals(_recallPowerUpSO, false);
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
            if (hit.collider.TryGetComponent<AnimalController>(out var animal)
                && animal.isSeated
                && animal.isRecallable)
            {

                RecallAnimal(animal);
            }
            else
            {
                DeactivateRecallMode();
            }
        }
        else
        {
            DeactivateRecallMode();
        }
    }
    /// <summary>
    /// Belirtilen hayvanı oturduğu yerden kaldırıp kuyruğun başına ekler.
    /// </summary>
    private void RecallAnimal(AnimalController animal)
    {
        animal.transform.DOKill();
        animal.transform.rotation = Quaternion.identity;
        if (animal.animalSO.eyepatched)
            animal.eyepatchRenderer.transform.position -= eyepatchOffset;

        foreach (var seat in animal.occupiedSeats)
            seat?.Vacate();
        animal.occupiedSeats.Clear();

        animal.isSeated = false;
        animal.animalSO.gridOriginPos = new Vector2Int(-1, -1);

        animalQueue.Insert(0, animal);

        animal.DisplayMyRules();

        DeactivateRecallMode();

        PowerUpController.Instance.DecreaseRemainingUse(_recallPowerUpSO);

        SoundManager.Instance.PlaySFX("RecallSuccess");
        Debug.Log($"{animal.animalSO._animalName} geri çağrıldı!");
    }

    public void ActivateEyepatchMode()
    {
        if (selectedAnimal != null || _eyepatchPowerUpSO.RemainingUse <= 0)
        {
            SoundManager.Instance.PlaySFX("EyepatchFail");
            return;
        }


        isEyepatchModeActive = true;
        PowerUpController.Instance.SetPowerUpVisuals(_eyepatchPowerUpSO, true);
        SoundManager.Instance.PlaySFX("PowerUpActivate");
        StartShakingSeatedAnimals(AnimalController.Instances.Where(animal => animal.animalSO._animalName == "Aslan" && !animal.animalSO.eyepatched));
        Debug.Log($"{_eyepatchPowerUpSO.powerupName} modu aktif. Geri alınacak hayvanı seçin.");
    }

    private void DeactivateEyepatchMode()
    {
        isEyepatchModeActive = false;
        PowerUpController.Instance.SetPowerUpVisuals(_eyepatchPowerUpSO, false);
        StopShakingSeatedAnimals();
    }

    public void HandleEyepatchClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, animalLayer))
        {
            if (hit.collider.TryGetComponent<AnimalController>(out var animal)
                && animal.animalSO._animalName == "Aslan"
                && !animal.animalSO.eyepatched)
            {
                EyepatchAnimal(animal);
            }
            else
            {
                DeactivateEyepatchMode();
            }
        }
        else
        {
            DeactivateEyepatchMode();
        }
    }

    private void EyepatchAnimal(AnimalController animal)
    {
        animal.transform.DOKill();
        animal.transform.rotation = Quaternion.identity;

        animal.animalSO.traits.Clear();
        animal.animalSO.eyepatched = true;

        DeactivateEyepatchMode();

        PowerUpController.Instance.DecreaseRemainingUse(_eyepatchPowerUpSO);

        SoundManager.Instance.PlaySFX("EyepatchSuccess");

        animal.eyepatchRenderer.enabled = true;
        if (animal.isSeated)
            animal.eyepatchRenderer.transform.position += eyepatchOffset;

        Debug.Log($"{animal.animalSO._animalName} gozu baglandi!");

    }
    private void StartShakingSeatedAnimals(IEnumerable<AnimalController> animals)
    {
        foreach (var animal in animals)
        {
            animal.transform.DOShakeRotation(
                duration: 2f,      // Bir tam sallanma döngüsü ne kadar sürsün (saniye).
                strength: 5f,      // Ne kadar güçlü sallanacağı (derece cinsinden).
                vibrato: 5,        // Ne kadar titreşimli/sık sallanacağı.
                randomness: 45f,   // Sallanmanın ne kadar rastgele olacağı (0-180).
                fadeOut: false     // Animasyon sonunda yavaşça durmasın.
            )
            .SetEase(Ease.InOutSine) // Yumuşak başla, yumuşak bitir.
            .SetLoops(-1, LoopType.Yoyo) // Sonsuz döngü ve Yoyo ile ileri-geri salınım.
            .SetId("shake");
        }
    }

    // <summary>
    // Hayvanlardaki tüm titreme animasyonlarını durdurur.
    // </summary>
    private void StopShakingSeatedAnimals()
    {
        DOTween.Kill("shake");

        foreach (var animal in AnimalController.Instances)
        {
            if (animal.isSeated)
            {
                animal.transform.DORotate(Vector3.zero, 0.1f);
            }
        }
    }

    private void StartPulsingSeatedAnimals(IEnumerable<AnimalController> animals)
    {
        float pulseScale = 1.15f;
        float pulseDuration = 0.5f;

        foreach (var animal in animals)
        {
            var t = animal.transform;
            t.DOKill();

            Vector3 originalScale = t.localScale;

            t.DOScale(originalScale * pulseScale, pulseDuration)
             .SetEase(Ease.InOutSine)
             .SetLoops(-1, LoopType.Yoyo)
             .SetId("pulse");
        }
    }

    private void StopPulsingSeatedAnimals()
    {
        DOTween.Kill("pulse");

        foreach (var animal in AnimalController.Instances)
        {
            if (animal.isSeated)
            {
                animal.transform.DOKill();
                animal.transform.localScale = Vector3.one;
            }
        }
    }

    #endregion
}
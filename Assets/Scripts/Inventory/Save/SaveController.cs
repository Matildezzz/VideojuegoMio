using Unity.Cinemachine;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class SaveController : MonoBehaviour
{
    public static SaveController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private ChestInventory[] chests;
    [SerializeField] private FarmPlot[] farmPlots;
    [SerializeField] private NPCFriendship[] npcFriendships;

    [Header("Save")]
    [SerializeField] private string fileName = "saveData.json";
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private bool autoSaveOnApplicationQuit = false;    

    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (chests == null || chests.Length == 0)
        {
            chests = FindObjectsByType<ChestInventory>(FindObjectsSortMode.None);
        }

        if (farmPlots == null || farmPlots.Length == 0)
        {
            RefreshFarmPlotReferences();
        }

        RefreshNPCFriendshipReferences();
    }

    private void Start()
    {
        if (autoLoadOnStart)
        {
            LoadGame();
        }
    }

    private void OnApplicationQuit()
    {
        if (autoSaveOnApplicationQuit)
        {
            SaveGame();
        }
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("SaveController: falta PlayerInventory.");
            return;
        }

        GameSaveData gameSave = new GameSaveData();

        gameSave.playerInventory = BuildPlayerInventorySaveData(playerInventory);
        gameSave.hasPlayerPosition = true;
        gameSave.playerPosition = playerInventory.transform.position;

        CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        if (confiner != null && confiner.BoundingShape2D != null)
        {
            gameSave.hasCameraBoundary = true;
            gameSave.cameraBoundaryName = confiner.BoundingShape2D.name;
        }

        if (CurrencyController.Instance != null)
        {
            gameSave.hasPlayerGold = true;
            gameSave.playerGold = CurrencyController.Instance.GetGold();
        }

        RefreshChestReferences();

        for (int i = 0; i < chests.Length; i++)
        {
            ChestInventory chest = chests[i];

            if (chest == null)
            {
                continue;
            }

            ChestSaveData chestSave = new ChestSaveData();
            chestSave.chestId = chest.ChestId;
            chestSave.container = BuildContainerSaveData(chest.Container);

            gameSave.chests.Add(chestSave);
        }

        if (TimeManager.Instance != null)
        {
            gameSave.hasTimeData = true;
            gameSave.day = TimeManager.Instance.Day;
            gameSave.hour = TimeManager.Instance.Hour;
            gameSave.minute = TimeManager.Instance.Minute;
        }

        RefreshFarmPlotReferences();

        for (int i = 0; i < farmPlots.Length; i++)
        {
            FarmPlot farmPlot = farmPlots[i];

            if (farmPlot == null)
            {
                continue;
            }

            gameSave.farmPlots.Add(farmPlot.CaptureSaveData());
        }

        if (MuseumController.Instance != null)
        {
            gameSave.museumDonatedItemIds = new List<string>(MuseumController.Instance.DonatedItemIds);
        }

        RefreshNPCFriendshipReferences();
        gameSave.npcFriendships.Clear();

        for (int i = 0; i < npcFriendships.Length; i++)
        {
            NPCFriendship friendship = npcFriendships[i];

            if (friendship == null)
            {
                continue;
            }

            gameSave.npcFriendships.Add(friendship.CaptureSaveData());
        }

        if (LanguageManager.Instance != null)
        {
            gameSave.language = LanguageManager.Instance.CaptureSaveData();
        }

        if (AlienDiaryManager.Instance != null)
        {
            gameSave.alienDiary = AlienDiaryManager.Instance.CaptureSaveData();
        }

        if (AlienNameManager.Instance != null)
        {
            gameSave.alienNames = AlienNameManager.Instance.CaptureSaveData();
        }

        if (QuestController.Instance != null)
        {
            gameSave.quests = QuestController.Instance.CaptureSaveData();
        }

        string json = JsonUtility.ToJson(gameSave, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Partida guardada en: " + SavePath);
    }

    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("SaveController: no existe archivo de guardado en " + SavePath);
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("SaveController: falta ItemDatabase.");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogError("SaveController: falta PlayerInventory.");
            return;
        }

        string json = File.ReadAllText(SavePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("SaveController: el archivo de guardado está vacío.");
            return;
        }

        GameSaveData gameSave = JsonUtility.FromJson<GameSaveData>(json);

        if (gameSave == null)
        {
            Debug.LogWarning("SaveController: no se pudo deserializar GameSaveData.");
            return;
        }

        LoadPlayerInventory(playerInventory, gameSave.playerInventory);

        if (gameSave.hasPlayerPosition)
        {
            playerInventory.transform.position = gameSave.playerPosition;
            Physics2D.SyncTransforms();
            RestoreCameraBoundary(gameSave);
        }

        if (gameSave.hasPlayerGold && CurrencyController.Instance != null)
        {
            CurrencyController.Instance.SetGold(gameSave.playerGold);
        }

        if (gameSave.hasTimeData && TimeManager.Instance != null)
        {
            TimeManager.Instance.LoadTime(gameSave.day, gameSave.hour, gameSave.minute);
        }

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.RestoreFromSaveData(gameSave.language);
        }

        if (AlienDiaryManager.Instance != null)
        {
            AlienDiaryManager.Instance.RestoreFromSaveData(gameSave.alienDiary);
        }

        if (AlienNameManager.Instance != null)
        {
            AlienNameManager.Instance.RestoreFromSaveData(gameSave.alienNames);
        }

        if (QuestController.Instance != null)
        {
            QuestController.Instance.RestoreFromSaveData(gameSave.quests);
        }

        if (MuseumController.Instance != null)
        {
            MuseumController.Instance.LoadDonations(gameSave.museumDonatedItemIds);
        }

        RefreshFarmPlotReferences();

        Dictionary<string, FarmPlot> farmPlotMap = new Dictionary<string, FarmPlot>();

        for (int i = 0; i < farmPlots.Length; i++)
        {
            FarmPlot farmPlot = farmPlots[i];

            if (farmPlot == null)
            {
                continue;
            }

            farmPlot.ResetPlotState();

            string plotKey = farmPlot.SaveKey;

            if (string.IsNullOrWhiteSpace(plotKey))
            {
                continue;
            }

            if (farmPlotMap.ContainsKey(plotKey))
            {
                Debug.LogWarning("SaveController: FarmPlot duplicado con key -> " + plotKey);
                continue;
            }

            farmPlotMap.Add(plotKey, farmPlot);
        }

        if (gameSave.farmPlots != null)
        {
            for (int i = 0; i < gameSave.farmPlots.Count; i++)
            {
                FarmPlotSaveData farmPlotSave = gameSave.farmPlots[i];

                if (farmPlotSave == null || string.IsNullOrWhiteSpace(farmPlotSave.plotKey))
                {
                    continue;
                }

                FarmPlot farmPlot;
                if (!farmPlotMap.TryGetValue(farmPlotSave.plotKey, out farmPlot))
                {
                    Debug.LogWarning("SaveController: no se encontró la parcela -> " + farmPlotSave.plotKey);
                    continue;
                }

                farmPlot.RestoreFromSaveData(farmPlotSave, itemDatabase);
            }
        }

        RefreshChestReferences();

        Dictionary<string, ChestInventory> chestMap = new Dictionary<string, ChestInventory>();

        for (int i = 0; i < chests.Length; i++)
        {
            ChestInventory chest = chests[i];

            if (chest == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(chest.ChestId))
            {
                Debug.LogWarning("SaveController: hay un cofre sin ChestId en escena.");
                continue;
            }

            if (chestMap.ContainsKey(chest.ChestId))
            {
                Debug.LogWarning("SaveController: ChestId duplicado en escena -> " + chest.ChestId);
                continue;
            }

            chestMap.Add(chest.ChestId, chest);
        }

        if (gameSave.chests != null)
        {
            for (int i = 0; i < gameSave.chests.Count; i++)
            {
                ChestSaveData chestSave = gameSave.chests[i];

                if (chestSave == null || string.IsNullOrWhiteSpace(chestSave.chestId))
                {
                    continue;
                }

                ChestInventory chest;
                if (!chestMap.TryGetValue(chestSave.chestId, out chest))
                {
                    Debug.LogWarning("SaveController: no se encontró en escena el cofre con id -> " + chestSave.chestId);
                    continue;
                }

                LoadContainer(chest.Container, chestSave.container);
            }
        }

        RefreshNPCFriendshipReferences();

        Dictionary<string, NPCFriendship> friendshipMap = new Dictionary<string, NPCFriendship>();

        for (int i = 0; i < npcFriendships.Length; i++)
        {
            NPCFriendship friendship = npcFriendships[i];

            if (friendship == null || string.IsNullOrWhiteSpace(friendship.NpcId))
            {
                continue;
            }

            if (friendshipMap.ContainsKey(friendship.NpcId))
            {
                Debug.LogWarning("SaveController: NPCFriendship duplicado con id -> " + friendship.NpcId);
                continue;
            }

            friendshipMap.Add(friendship.NpcId, friendship);
        }

        if (gameSave.npcFriendships != null)
        {
            for (int i = 0; i < gameSave.npcFriendships.Count; i++)
            {
                NPCFriendshipSaveData saveData = gameSave.npcFriendships[i];

                if (saveData == null || string.IsNullOrWhiteSpace(saveData.npcId))
                {
                    continue;
                }

                NPCFriendship friendship;
                if (!friendshipMap.TryGetValue(saveData.npcId, out friendship))
                {
                    continue;
                }

                friendship.LoadData(saveData.friendshipPoints, saveData.lastTalkDay, saveData.lastGiftDay);
            }
        }

        Debug.Log("Partida cargada desde: " + SavePath);
    }

    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("SaveController: archivo borrado -> " + SavePath);
        }
    }

    public bool HasSave()
    {
        return File.Exists(SavePath);
    }

    private PlayerInventorySaveData BuildPlayerInventorySaveData(PlayerInventory inventory)
    {
        PlayerInventorySaveData saveData = new PlayerInventorySaveData();

        saveData.hotbar = BuildContainerSaveData(inventory.Hotbar);
        saveData.backpack = BuildContainerSaveData(inventory.Backpack);
        saveData.selectedHotbarIndex = inventory.SelectedHotbarIndex;

        return saveData;
    }

    private ContainerSaveData BuildContainerSaveData(InventoryContainer container)
    {
        ContainerSaveData saveData = new ContainerSaveData();

        if (container == null)
        {
            return saveData;
        }

        for (int i = 0; i < container.SlotCount; i++)
        {
            InventorySlot slot = container.GetSlot(i);
            SlotSaveData slotSave = new SlotSaveData();

            if (slot != null && !slot.IsEmpty)
            {
                slotSave.itemId = slot.Item.ItemId;
                slotSave.amount = slot.Amount;
            }
            else
            {
                slotSave.itemId = string.Empty;
                slotSave.amount = 0;
            }

            saveData.slots.Add(slotSave);
        }

        return saveData;
    }

    private void LoadPlayerInventory(PlayerInventory inventory, PlayerInventorySaveData saveData)
    {
        if (inventory == null)
        {
            return;
        }

        if (saveData == null)
        {
            ClearContainer(inventory.Hotbar);
            ClearContainer(inventory.Backpack);
            inventory.SelectSlot(0);
            return;
        }

        LoadContainer(inventory.Hotbar, saveData.hotbar);
        LoadContainer(inventory.Backpack, saveData.backpack);

        int maxIndex = Mathf.Max(0, inventory.Hotbar.SlotCount - 1);
        int selectedIndex = Mathf.Clamp(saveData.selectedHotbarIndex, 0, maxIndex);
        inventory.SelectSlot(selectedIndex);
    }

    private void LoadContainer(InventoryContainer container, ContainerSaveData saveData)
    {
        if (container == null)
        {
            return;
        }

        ClearContainer(container);

        if (saveData == null || saveData.slots == null)
        {
            container.ForceNotifyChanged();
            return;
        }

        int count = Mathf.Min(container.SlotCount, saveData.slots.Count);

        for (int i = 0; i < count; i++)
        {
            SlotSaveData slotSave = saveData.slots[i];
            InventorySlot runtimeSlot = container.GetSlot(i);

            if (runtimeSlot == null)
            {
                continue;
            }

            if (slotSave == null || string.IsNullOrWhiteSpace(slotSave.itemId) || slotSave.amount <= 0)
            {
                runtimeSlot.Clear();
                continue;
            }

            ItemData itemData = itemDatabase.GetItemById(slotSave.itemId);

            if (itemData == null)
            {
                Debug.LogWarning("SaveController: no existe item con id -> " + slotSave.itemId);
                runtimeSlot.Clear();
                continue;
            }

            int maxAmount = itemData.Stackable ? itemData.MaxStack : 1;

            runtimeSlot.Item = itemData;
            runtimeSlot.Amount = Mathf.Clamp(slotSave.amount, 1, maxAmount);
        }

        container.ForceNotifyChanged();
    }

    private void ClearContainer(InventoryContainer container)
    {
        if (container == null)
        {
            return;
        }

        for (int i = 0; i < container.SlotCount; i++)
        {
            InventorySlot slot = container.GetSlot(i);

            if (slot == null)
            {
                continue;
            }

            slot.Clear();
        }
    }

    private void RestoreCameraBoundary(GameSaveData gameSave)
    {
        CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();

        if (confiner == null)
        {
            return;
        }

        PolygonCollider2D boundary = null;

        if (gameSave != null && gameSave.hasCameraBoundary && !string.IsNullOrWhiteSpace(gameSave.cameraBoundaryName))
        {
            PolygonCollider2D[] colliders = FindObjectsByType<PolygonCollider2D>(FindObjectsSortMode.None);

            for (int i = 0; i < colliders.Length; i++)
            {
                PolygonCollider2D candidate = colliders[i];

                if (candidate == null)
                {
                    continue;
                }

                if (candidate.name == gameSave.cameraBoundaryName)
                {
                    boundary = candidate;
                    break;
                }
            }
        }

        if (boundary == null)
        {
            Vector2 playerPosition = playerInventory.transform.position;
            PolygonCollider2D[] colliders = FindObjectsByType<PolygonCollider2D>(FindObjectsSortMode.None);

            for (int i = 0; i < colliders.Length; i++)
            {
                PolygonCollider2D candidate = colliders[i];

                if (candidate == null || !candidate.isTrigger)
                {
                    continue;
                }

                if (candidate.OverlapPoint(playerPosition))
                {
                    boundary = candidate;
                    break;
                }
            }
        }

        if (boundary == null)
        {
            Debug.LogWarning("SaveController: no se pudo restaurar el boundary de cámara tras cargar la partida.");
            return;
        }

        confiner.BoundingShape2D = boundary;
        confiner.InvalidateBoundingShapeCache();
        MapController_Manual.Instance?.HighlightArea(boundary.name);
    }

    private void RefreshChestReferences()
    {
        chests = FindObjectsByType<ChestInventory>(FindObjectsSortMode.None);
    }

    private void RefreshFarmPlotReferences()
    {
        farmPlots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
    }

    private void RefreshNPCFriendshipReferences()
    {
        npcFriendships = FindObjectsByType<NPCFriendship>(FindObjectsSortMode.None);
    }
}
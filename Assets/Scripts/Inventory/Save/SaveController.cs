using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class SaveController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private ChestInventory[] chests;

    [Header("Save")]
    [SerializeField] private string fileName = "saveData.json";
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private bool autoSaveOnApplicationQuit = false;

    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (chests == null || chests.Length == 0)
        {
            chests = FindObjectsByType<ChestInventory>(FindObjectsSortMode.None);
        }
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

    private void RefreshChestReferences()
    {
        chests = FindObjectsByType<ChestInventory>(FindObjectsSortMode.None);
    }
}
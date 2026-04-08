using System.IO;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Linq;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private Chest[] chests;
    // private ShopNPC[] shops; // Track shops in scene

    void Start()
    {   
        InitializeComponents();        
        LoadGame();
    }

    private void InitializeComponents()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindAnyObjectByType<InventoryController>();
        hotbarController = FindAnyObjectByType<HotbarController>();
        chests = FindObjectsByType<Chest>(FindObjectsSortMode.None);
        // shops = FindObjectsByType<ShopNPC>(FindObjectsSortMode.None);
    }

    public void SaveGame()
    {
        CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();

        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundry = confiner.BoundingShape2D.gameObject.name,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData = hotbarController.GetHotbarItems(),
            chestSaveData = GetChestsState(),
            questProgressData = QuestController.Instance.activeQuests,
            handinQuestIDs = QuestController.Instance.handinQuestIDs,
            // playerGold = CurrencyController.Instance.GetGold(),
            // shopStates = GetShopStates()
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    /*private List<ShopInstanceData> GetShopStates()
    {
        List<ShopInstanceData> shopStates = new List<ShopInstanceData>();
        foreach(var shop in shops)
        {
            ShopInstanceData shopData = new ShopInstanceData
            {
                shopID = shop.shopID,
                stock = new List<ShopItemData>()
            };

            foreach(var stockItem in shop.GetCurrentStock())
            {
                shopData.stock.Add(new ShopItemData
                {
                    itemID = stockItem.itemID,
                    quantity = stockItem.quantity
                });
            }

            shopStates.Add(shopData);
        }
        return shopStates;
    }*/

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestsStates = new List<ChestSaveData>();

        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData = new ChestSaveData
            {
                chestID = chest.ChestID,
                IsOpened = chest.IsOpened
            };
            chestsStates.Add(chestSaveData);
        }
        return chestsStates;
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;
            MapController_Manual.Instance?.HighlightArea(saveData.mapBoundry);

            CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
            confiner.BoundingShape2D =
                GameObject.Find(saveData.mapBoundry).GetComponent<PolygonCollider2D>();

            confiner.InvalidateBoundingShapeCache();

            inventoryController.SetInventoryItems(saveData.inventorySaveData);
            hotbarController.SetHotbarItems(saveData.hotbarSaveData);

            LoadChestStates(saveData.chestSaveData);

            // LoadShopStates(saveData.shopStates);
            // CurrencyController.Instance.SetGold(saveData.playerGold);

            QuestController.Instance.LoadQuestProgress(saveData.questProgressData);
            QuestController.Instance.handinQuestIDs = saveData.handinQuestIDs;
        }
        else
        {
            SaveGame();

            inventoryController.SetInventoryItems(new List<InventorySaveData>());
            hotbarController.SetHotbarItems(new List<InventorySaveData>());
        }
    }

    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData = chestStates.FirstOrDefault( c => c.chestID == chest.ChestID);

            if (chestSaveData != null)
            {
                chest.SetOpened(chestSaveData.IsOpened);
            }
        }
    }

    /*private void LoadShopStates(List<ShopInstanceData> shopStates)
    {
        if(shopStates == null) return;

        foreach(var shop in shops)
        {
            ShopInstanceData shopData = shopStates.FirstOrDefault(s => s.shopID == shop.shopID);

            if(shopData != null)
            {
                List<ShopNPC.ShopStockItem> loadedStock = new List<ShopNPC.ShopStockItem>();

                foreach(var itemData in shopData.stock)
                {
                    loadedStock.Add(new ShopNPC.ShopStockItem
                    {
                        itemID = itemData.itemID,
                        quantity = itemData.quantity
                    });
                }
                shop.SetStock(loadedStock);
            }
        }
    }*/
}
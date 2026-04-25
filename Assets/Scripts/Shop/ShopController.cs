using TMPro;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    public static ShopController Instance;

    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("UI")]
    public GameObject shopPanel;
    public Transform shopInventoryGrid;
    public Transform playerInventoryGrid;
    public GameObject shopSlotPrefab;
    [SerializeField] private GameObject shopItemUIPrefab;
    public TMP_Text playerMoneyText;
    public TMP_Text shopTitleText;

    private ShopNPC currentShop;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }
    }

    private void Start()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += HandlePlayerInventoryChanged;
        }

        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.OnGoldChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(CurrencyController.Instance.GetGold());
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("ShopController: falta asignar ItemDatabase.");
        }
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= HandlePlayerInventoryChanged;
        }

        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.OnGoldChanged -= UpdateMoneyDisplay;
        }
    }

    private void HandlePlayerInventoryChanged()
    {
        if (shopPanel != null && shopPanel.activeSelf)
        {
            RefreshPlayerInventoryDisplay();
        }
    }

    private void UpdateMoneyDisplay(int amount)
    {
        if (playerMoneyText != null)
        {
            playerMoneyText.text = amount.ToString();
        }
    }

    public void OpenShop(ShopNPC shop)
    {
        if (shop == null)
        {
            return;
        }

        currentShop = shop;

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        if (shopTitleText != null)
        {
            shopTitleText.text = shop.shopkeeperName + "'s Shop";
        }

        RefreshShopDisplay();
        RefreshPlayerInventoryDisplay();
        PauseController.SetPause(true);
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        currentShop = null;
        PauseController.SetPause(false);
    }

    public void RefreshShopDisplay()
    {
        if (currentShop == null || shopInventoryGrid == null)
        {
            return;
        }

        ClearGrid(shopInventoryGrid);

        foreach (var stockItem in currentShop.GetCurrentStock())
        {
            if (stockItem == null || stockItem.quantity <= 0 || string.IsNullOrWhiteSpace(stockItem.itemId))
            {
                continue;
            }

            ItemData itemData = itemDatabase != null ? itemDatabase.GetItemById(stockItem.itemId) : null;
            if (itemData == null)
            {
                continue;
            }

            CreateShopSlot(shopInventoryGrid, itemData, stockItem.quantity, true, InventoryUISlotSource.Backpack, -1);
        }
    }

    public void RefreshPlayerInventoryDisplay()
    {
        if (playerInventory == null || playerInventoryGrid == null)
        {
            return;
        }

        ClearGrid(playerInventoryGrid);

        AddContainerToPlayerDisplay(playerInventory.Hotbar, InventoryUISlotSource.Hotbar);
        AddContainerToPlayerDisplay(playerInventory.Backpack, InventoryUISlotSource.Backpack);
    }

    private void AddContainerToPlayerDisplay(InventoryContainer container, InventoryUISlotSource source)
    {
        if (container == null)
        {
            return;
        }

        for (int i = 0; i < container.SlotCount; i++)
        {
            InventorySlot slot = container.GetSlot(i);

            if (slot == null || slot.IsEmpty)
            {
                continue;
            }

            CreateShopSlot(playerInventoryGrid, slot.Item, slot.Amount, false, source, i);
        }
    }

    private void CreateShopSlot(
        Transform grid,
        ItemData itemData,
        int quantity,
        bool isShop,
        InventoryUISlotSource source,
        int slotIndex)
    {
        if (grid == null || shopSlotPrefab == null || itemData == null || quantity <= 0)
        {
            return;
        }

        GameObject slotObj = Instantiate(shopSlotPrefab, grid);
        GameObject itemInstance = CreateShopItemVisual(slotObj.transform, itemData, quantity);

        if (itemInstance == null)
        {
            Destroy(slotObj);
            return;
        }

        int price = isShop ? itemData.BuyPrice : itemData.GetSellPrice();

        ShopSlot slot = slotObj.GetComponent<ShopSlot>();
        if (slot != null)
        {
            slot.isShopSlot = isShop;
            slot.SetItem(itemInstance, price);
        }

        ShopItemHandler handler = itemInstance.GetComponent<ShopItemHandler>();
        if (handler == null)
        {
            handler = itemInstance.AddComponent<ShopItemHandler>();
        }

        handler.Initialise(isShop, itemData, source, slotIndex);
    }

    private GameObject CreateShopItemVisual(Transform parent, ItemData itemData, int quantity)
    {
        if (shopItemUIPrefab == null)
        {
            Debug.LogWarning("ShopController: falta asignar shopItemUIPrefab.");
            return null;
        }

        GameObject itemObj = Instantiate(shopItemUIPrefab, parent);

        RectTransform rt = itemObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        ShopItemVisual itemUI = itemObj.GetComponent<ShopItemVisual>();
        if (itemUI == null)
        {
            itemUI = itemObj.AddComponent<ShopItemVisual>();
        }

        itemUI.Configure(itemData, quantity);

        return itemObj;
    }

    public bool TryBuyItem(ItemData itemData, int amount = 1)
    {
        if (currentShop == null || itemData == null || amount <= 0)
        {
            return false;
        }

        if (CurrencyController.Instance == null)
        {
            return false;
        }

        if (playerInventory == null)
        {
            return false;
        }

        int totalPrice = itemData.BuyPrice * amount;

        if (CurrencyController.Instance.GetGold() < totalPrice)
        {
            Debug.Log("Not enough gold!");

            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("No tienes suficiente oro", ToastType.Error, "Error");
            }

            return false;
        }

        if (!playerInventory.CanAddItem(itemData, amount))
        {
            Debug.Log("Inventory full!");

            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("Inventario lleno", ToastType.Error, "Error");
            }

            return false;
        }

        if (!currentShop.RemoveFromShopStock(itemData.ItemId, amount))
        {
            return false;
        }

        if (!CurrencyController.Instance.SpendGold(totalPrice))
        {
            currentShop.AddToStock(itemData.ItemId, amount);
            return false;
        }

        if (!playerInventory.TryAddItem(itemData, amount))
        {
            CurrencyController.Instance.AddGold(totalPrice);
            currentShop.AddToStock(itemData.ItemId, amount);
            return false;
        }

        RefreshShopDisplay();
        RefreshPlayerInventoryDisplay();

        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("Has comprado " + itemData.DisplayName + " x" + amount, ToastType.Success, "Coin");
        }

        return true;
    }

    public bool TrySellItem(ItemData itemData, InventoryUISlotSource source, int slotIndex, int amount = 1)
    {
        if (currentShop == null || playerInventory == null || itemData == null || amount <= 0)
        {
            return false;
        }

        InventoryContainer container = GetPlayerContainer(source);
        if (container == null)
        {
            return false;
        }

        InventorySlot slot = container.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty)
        {
            return false;
        }

        if (slot.Item != itemData || slot.Amount < amount)
        {
            return false;
        }

        if (!container.RemoveFromSlot(slotIndex, amount))
        {
            return false;
        }

        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.AddGold(itemData.GetSellPrice() * amount);
        }

        currentShop.AddToStock(itemData.ItemId, amount);

        RefreshShopDisplay();
        RefreshPlayerInventoryDisplay();

        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("Has vendido " + itemData.DisplayName + " x" + amount, ToastType.Success, "Coin");
        }

        return true;
    }

    private InventoryContainer GetPlayerContainer(InventoryUISlotSource source)
    {
        switch (source)
        {
            case InventoryUISlotSource.Hotbar:
                return playerInventory.Hotbar;

            case InventoryUISlotSource.Backpack:
                return playerInventory.Backpack;

            default:
                return null;
        }
    }

    private void ClearGrid(Transform grid)
    {
        if (grid == null)
        {
            return;
        }

        for (int i = grid.childCount - 1; i >= 0; i--)
        {
            Destroy(grid.GetChild(i).gameObject);
        }
    }

    public void AddItemToShop(string itemId, int quantity)
    {
        if (currentShop == null)
        {
            return;
        }

        currentShop.AddToStock(itemId, quantity);
        RefreshShopDisplay();
    }

    public bool RemoveItemFromShop(string itemId, int quantity)
    {
        if (currentShop == null)
        {
            return false;
        }

        bool success = currentShop.RemoveFromShopStock(itemId, quantity);

        if (success)
        {
            RefreshShopDisplay();
        }

        return success;
    }
}
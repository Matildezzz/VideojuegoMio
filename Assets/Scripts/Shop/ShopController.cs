using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    public static ShopController Instance;

    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("UI principal")]
    public GameObject shopPanel;
    public Transform shopInventoryGrid;
    public Transform playerInventoryGrid;
    public GameObject shopSlotPrefab;
    [SerializeField] private GameObject shopItemUIPrefab;
    public TMP_Text playerMoneyText;
    public TMP_Text shopTitleText;

    [Header("Panel de detalle")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailIconImage;
    [SerializeField] private TMP_Text detailNameText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private TMP_Text detailPriceText;
    [SerializeField] private TMP_Text detailStockText;
    [SerializeField] private TMP_Text detailModeText;
    [SerializeField] private TMP_Text detailGoldText;
    [SerializeField] private Button buyOneButton;
    [SerializeField] private Button buyFiveButton;
    [SerializeField] private Button sellOneButton;
    [SerializeField] private Button sellFiveButton;
    [SerializeField] private Button closeDetailButton;

    [Header("Confirmacion de compra cara")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private TMP_Text confirmationText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private int expensivePurchaseThreshold = 100;

    private ShopNPC currentShop;

    private ItemData selectedItem;
    private bool selectedIsShopItem;
    private InventoryUISlotSource selectedSource = InventoryUISlotSource.Backpack;
    private int selectedSlotIndex = -1;
    private int selectedAvailableAmount = 0;

    private bool pendingIsBuy;
    private int pendingAmount;

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

        HideDetail();
        HideConfirmation();
        BindButtons();

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

        UnbindButtons();
    }

    private void BindButtons()
    {
        if (buyOneButton != null) buyOneButton.onClick.AddListener(BuyOne);
        if (buyFiveButton != null) buyFiveButton.onClick.AddListener(BuyFive);
        if (sellOneButton != null) sellOneButton.onClick.AddListener(SellOne);
        if (sellFiveButton != null) sellFiveButton.onClick.AddListener(SellFive);
        if (closeDetailButton != null) closeDetailButton.onClick.AddListener(HideDetail);
        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmPendingTransaction);
        if (cancelButton != null) cancelButton.onClick.AddListener(CancelPendingTransaction);
    }

    private void UnbindButtons()
    {
        if (buyOneButton != null) buyOneButton.onClick.RemoveListener(BuyOne);
        if (buyFiveButton != null) buyFiveButton.onClick.RemoveListener(BuyFive);
        if (sellOneButton != null) sellOneButton.onClick.RemoveListener(SellOne);
        if (sellFiveButton != null) sellFiveButton.onClick.RemoveListener(SellFive);
        if (closeDetailButton != null) closeDetailButton.onClick.RemoveListener(HideDetail);
        if (confirmButton != null) confirmButton.onClick.RemoveListener(ConfirmPendingTransaction);
        if (cancelButton != null) cancelButton.onClick.RemoveListener(CancelPendingTransaction);
    }

    private void HandlePlayerInventoryChanged()
    {
        if (shopPanel != null && shopPanel.activeSelf)
        {
            RefreshPlayerInventoryDisplay();
            RefreshSelectedDetail();
        }
    }

    private void UpdateMoneyDisplay(int amount)
    {
        string text = amount + " oro";

        if (playerMoneyText != null)
        {
            playerMoneyText.text = text;
        }

        if (detailGoldText != null)
        {
            detailGoldText.text = text;
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
        HideDetail();
        HideConfirmation();

        if (CurrencyController.Instance != null)
        {
            UpdateMoneyDisplay(CurrencyController.Instance.GetGold());
        }

        PauseController.SetPause(true);
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        currentShop = null;
        HideDetail();
        HideConfirmation();
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
            slot.SetItem(itemInstance, price, isShop);
        }

        ShopItemHandler handler = itemInstance.GetComponent<ShopItemHandler>();
        if (handler == null)
        {
            handler = itemInstance.AddComponent<ShopItemHandler>();
        }

        handler.Initialise(isShop, itemData, source, slotIndex, quantity);
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

    public void SelectShopItem(ItemData itemData, int availableAmount = 1)
    {
        SelectItem(itemData, true, InventoryUISlotSource.Backpack, -1, availableAmount);
    }

    public void SelectPlayerItem(ItemData itemData, InventoryUISlotSource source, int slotIndex, int availableAmount = 1)
    {
        SelectItem(itemData, false, source, slotIndex, availableAmount);
    }

    private void SelectItem(ItemData itemData, bool isShopItem, InventoryUISlotSource source, int slotIndex, int availableAmount)
    {
        if (itemData == null)
        {
            HideDetail();
            return;
        }

        selectedItem = itemData;
        selectedIsShopItem = isShopItem;
        selectedSource = source;
        selectedSlotIndex = slotIndex;
        selectedAvailableAmount = Mathf.Max(1, availableAmount);

        RefreshSelectedDetail();
    }

    private void RefreshSelectedDetail()
    {
        if (selectedItem == null)
        {
            HideDetail();
            return;
        }

        if (!IsSelectedItemStillAvailable())
        {
            HideDetail();
            return;
        }

        if (detailPanel != null)
        {
            detailPanel.SetActive(true);
        }

        if (detailIconImage != null)
        {
            detailIconImage.sprite = selectedItem.Icon;
            detailIconImage.enabled = selectedItem.Icon != null;
            detailIconImage.preserveAspect = true;
        }

        if (detailNameText != null)
        {
            detailNameText.text = selectedItem.DisplayName;
        }

        if (detailDescriptionText != null)
        {
            detailDescriptionText.text = string.IsNullOrWhiteSpace(selectedItem.Description)
                ? GetFallbackDescription(selectedItem)
                : selectedItem.Description;
        }

        if (detailModeText != null)
        {
            detailModeText.text = selectedIsShopItem ? "Producto de tienda" : "Objeto del inventario";
        }

        if (detailPriceText != null)
        {
            int price = selectedIsShopItem ? selectedItem.BuyPrice : selectedItem.GetSellPrice();
            detailPriceText.text = selectedIsShopItem ? "Precio: " + price + " oro" : "Venta: " + price + " oro";
        }

        if (detailStockText != null)
        {
            detailStockText.text = selectedIsShopItem
                ? "Stock: " + selectedAvailableAmount
                : "Tienes: " + selectedAvailableAmount;
        }

        bool canBuy = selectedIsShopItem;
        bool canSell = !selectedIsShopItem;

        if (buyOneButton != null) buyOneButton.gameObject.SetActive(canBuy);
        if (buyFiveButton != null) buyFiveButton.gameObject.SetActive(canBuy);
        if (sellOneButton != null) sellOneButton.gameObject.SetActive(canSell);
        if (sellFiveButton != null) sellFiveButton.gameObject.SetActive(canSell);

        if (CurrencyController.Instance != null)
        {
            UpdateMoneyDisplay(CurrencyController.Instance.GetGold());
        }
    }

    private bool IsSelectedItemStillAvailable()
    {
        if (selectedIsShopItem)
        {
            selectedAvailableAmount = GetShopStockAmount(selectedItem.ItemId);
            return selectedAvailableAmount > 0;
        }

        InventoryContainer container = GetPlayerContainer(selectedSource);
        if (container == null)
        {
            return false;
        }

        InventorySlot slot = container.GetSlot(selectedSlotIndex);
        if (slot == null || slot.IsEmpty || slot.Item != selectedItem)
        {
            return false;
        }

        selectedAvailableAmount = slot.Amount;
        return selectedAvailableAmount > 0;
    }

    private int GetShopStockAmount(string itemId)
    {
        if (currentShop == null || string.IsNullOrWhiteSpace(itemId))
        {
            return 0;
        }

        foreach (var stockItem in currentShop.GetCurrentStock())
        {
            if (stockItem != null && stockItem.itemId == itemId)
            {
                return stockItem.quantity;
            }
        }

        return 0;
    }

    private string GetFallbackDescription(ItemData itemData)
    {
        if (itemData == null)
        {
            return string.Empty;
        }

        return "Objeto del juego. Revisa su precio, stock y uso antes de comprarlo o venderlo.";
    }

    public void HideDetail()
    {
        selectedItem = null;
        selectedAvailableAmount = 0;
        selectedSlotIndex = -1;

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    private void HideConfirmation()
    {
        pendingAmount = 0;

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    public void BuyOne()
    {
        TryBuySelected(1);
    }

    public void BuyFive()
    {
        TryBuySelected(5);
    }

    public void SellOne()
    {
        TrySellSelected(1);
    }

    public void SellFive()
    {
        TrySellSelected(5);
    }

    private void TryBuySelected(int amount)
    {
        if (selectedItem == null || !selectedIsShopItem)
        {
            ShowWarning("Selecciona un producto de la tienda.");
            return;
        }

        int finalAmount = Mathf.Min(amount, selectedAvailableAmount);
        if (finalAmount <= 0)
        {
            ShowWarning("La tienda no tiene stock de este producto.");
            return;
        }

        int totalPrice = selectedItem.BuyPrice * finalAmount;
        if (totalPrice >= expensivePurchaseThreshold && confirmationPanel != null)
        {
            ShowConfirmation(true, finalAmount, "Vas a comprar " + selectedItem.DisplayName + " x" + finalAmount + " por " + totalPrice + " oro. ¿Seguro?");
            return;
        }

        TryBuyItem(selectedItem, finalAmount);
    }

    private void TrySellSelected(int amount)
    {
        if (selectedItem == null || selectedIsShopItem)
        {
            ShowWarning("Selecciona un objeto de tu inventario.");
            return;
        }

        int finalAmount = Mathf.Min(amount, selectedAvailableAmount);
        if (finalAmount <= 0)
        {
            ShowWarning("No tienes unidades para vender.");
            return;
        }

        TrySellItem(selectedItem, selectedSource, selectedSlotIndex, finalAmount);
    }

    private void ShowConfirmation(bool isBuy, int amount, string message)
    {
        pendingIsBuy = isBuy;
        pendingAmount = amount;

        if (confirmationText != null)
        {
            confirmationText.text = message;
        }

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
    }

    public void ConfirmPendingTransaction()
    {
        if (selectedItem == null || pendingAmount <= 0)
        {
            HideConfirmation();
            return;
        }

        int amount = pendingAmount;
        bool isBuy = pendingIsBuy;
        HideConfirmation();

        if (isBuy)
        {
            TryBuyItem(selectedItem, amount);
        }
        else
        {
            TrySellItem(selectedItem, selectedSource, selectedSlotIndex, amount);
        }
    }

    public void CancelPendingTransaction()
    {
        HideConfirmation();
        ShowWarning("Compra cancelada.");
    }

    public bool TryBuyItem(ItemData itemData, int amount = 1)
    {
        if (currentShop == null)
        {
            ShowError("No hay ninguna tienda abierta.");
            return false;
        }

        if (itemData == null || amount <= 0)
        {
            ShowError("Este producto no está configurado.");
            return false;
        }

        if (CurrencyController.Instance == null)
        {
            ShowError("No se encontró el sistema de monedas.");
            return false;
        }

        if (playerInventory == null)
        {
            ShowError("No se encontró el inventario del jugador.");
            return false;
        }

        int totalPrice = itemData.BuyPrice * amount;

        if (CurrencyController.Instance.GetGold() < totalPrice)
        {
            Debug.Log("Not enough gold!");
            ShowError("No tienes suficiente oro.");
            return false;
        }

        if (!playerInventory.CanAddItem(itemData, amount))
        {
            Debug.Log("Inventory full!");
            ShowError("No hay espacio en el inventario.");
            return false;
        }

        if (!currentShop.RemoveFromShopStock(itemData.ItemId, amount))
        {
            ShowWarning("La tienda no tiene suficiente stock.");
            RefreshSelectedDetail();
            return false;
        }

        if (!CurrencyController.Instance.SpendGold(totalPrice))
        {
            currentShop.AddToStock(itemData.ItemId, amount);
            ShowError("No tienes suficiente oro.");
            return false;
        }

        if (!playerInventory.TryAddItem(itemData, amount))
        {
            CurrencyController.Instance.AddGold(totalPrice);
            currentShop.AddToStock(itemData.ItemId, amount);
            ShowError("No hay espacio en el inventario.");
            return false;
        }

        RefreshShopDisplay();
        RefreshPlayerInventoryDisplay();
        RefreshSelectedDetail();

        ShowSuccess("Has comprado " + itemData.DisplayName + " x" + amount + " por " + totalPrice + " oro", "Coin");

        return true;
    }

    public bool TrySellItem(ItemData itemData, InventoryUISlotSource source, int slotIndex, int amount = 1)
    {
        if (currentShop == null)
        {
            ShowError("No hay ninguna tienda abierta.");
            return false;
        }

        if (playerInventory == null)
        {
            ShowError("No se encontró el inventario del jugador.");
            return false;
        }

        if (itemData == null || amount <= 0)
        {
            ShowError("Selecciona un objeto válido para vender.");
            return false;
        }

        InventoryContainer container = GetPlayerContainer(source);
        if (container == null)
        {
            ShowError("No se encontró el contenedor del inventario.");
            return false;
        }

        InventorySlot slot = container.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty)
        {
            ShowWarning("No hay ningún objeto en ese slot.");
            RefreshSelectedDetail();
            return false;
        }

        if (slot.Item != itemData || slot.Amount < amount)
        {
            ShowWarning("No tienes suficientes unidades para vender.");
            RefreshSelectedDetail();
            return false;
        }

        if (!container.RemoveFromSlot(slotIndex, amount))
        {
            ShowError("No se pudo quitar el objeto del inventario.");
            return false;
        }

        int totalGold = itemData.GetSellPrice() * amount;

        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.AddGold(totalGold);
        }
        else
        {
            ShowWarning("Se vendió el objeto, pero no se encontró el sistema de monedas.");
        }

        currentShop.AddToStock(itemData.ItemId, amount);

        RefreshShopDisplay();
        RefreshPlayerInventoryDisplay();
        RefreshSelectedDetail();

        ShowSuccess("Has vendido " + itemData.DisplayName + " x" + amount + " por " + totalGold + " oro", "Coin");

        return true;
    }

    private void ShowSuccess(string message, string soundName)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Success, soundName);
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void ShowWarning(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Warning, "Error");
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void ShowError(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Error, "Error");
        }
        else
        {
            Debug.Log(message);
        }
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
        RefreshSelectedDetail();
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
            RefreshSelectedDetail();
        }

        return success;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ChestUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ChestTransferController chestTransferController;
    [SerializeField] private GameObject chestUIRoot;

    [Header("Player UI")]
    [SerializeField] private List<InventorySlotUI> hotbarSlotUIs = new List<InventorySlotUI>();
    [SerializeField] private List<InventorySlotUI> backpackSlotUIs = new List<InventorySlotUI>();

    [Header("Chest UI")]
    [SerializeField] private List<InventorySlotUI> chestSlotUIs = new List<InventorySlotUI>();

    private ChestInventory currentChest;

    private InventoryUISlotSource draggingSource;
    private int draggingSlotIndex = -1;
    private Image dragIconImage;
    private RectTransform dragIconRect;

    public bool IsOpen => chestUIRoot != null && chestUIRoot.activeSelf;
    public ChestInventory CurrentChest => currentChest;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }
    }

    private void OnEnable()
    {
        SubscribePlayer();
        RefreshAll();
    }

    private void OnDisable()
    {
        UnsubscribePlayer();
        UnsubscribeChest();
        HideDragIcon();
        draggingSlotIndex = -1;
        PauseController.SetPause(false);
    }

    public void OpenChest(ChestInventory chest)
    {
        if (chest == null)
        {
            return;
        }

        if (currentChest == chest)
        {
            if (chestUIRoot != null)
            {
                chestUIRoot.SetActive(true);
            }

            PauseController.SetPause(true);
            RefreshAll();
            return;
        }

        UnsubscribeChest();

        currentChest = chest;
        currentChest.Open();

        if (chestTransferController != null)
        {
            chestTransferController.SetCurrentChest(currentChest);
        }

        SubscribeChest();

        if (chestUIRoot != null)
        {
            chestUIRoot.SetActive(true);
        }

        PauseController.SetPause(true);
        RefreshAll();
    }

    public void CloseChest()
    {
        if (currentChest != null)
        {
            currentChest.Close();
        }

        UnsubscribeChest();

        if (chestTransferController != null)
        {
            chestTransferController.ClearCurrentChest();
        }

        currentChest = null;
        HideDragIcon();
        draggingSlotIndex = -1;

        if (chestUIRoot != null)
        {
            chestUIRoot.SetActive(false);
        }

        PauseController.SetPause(false);
    }

    private void SubscribePlayer()
    {
        if (playerInventory == null)
        {
            return;
        }

        playerInventory.OnInventoryChanged -= HandleAnyInventoryChanged;
        playerInventory.OnInventoryChanged += HandleAnyInventoryChanged;

        playerInventory.OnSelectedHotbarChanged -= HandleSelectedHotbarChanged;
        playerInventory.OnSelectedHotbarChanged += HandleSelectedHotbarChanged;
    }

    private void UnsubscribePlayer()
    {
        if (playerInventory == null)
        {
            return;
        }

        playerInventory.OnInventoryChanged -= HandleAnyInventoryChanged;
        playerInventory.OnSelectedHotbarChanged -= HandleSelectedHotbarChanged;
    }

    private void SubscribeChest()
    {
        if (currentChest == null)
        {
            return;
        }

        currentChest.OnContainerChanged -= HandleAnyInventoryChanged;
        currentChest.OnContainerChanged += HandleAnyInventoryChanged;
    }

    private void UnsubscribeChest()
    {
        if (currentChest == null)
        {
            return;
        }

        currentChest.OnContainerChanged -= HandleAnyInventoryChanged;
    }

    private void HandleAnyInventoryChanged()
    {
        RefreshAll();
    }

    private void HandleSelectedHotbarChanged(int selectedIndex)
    {
        RefreshPlayerHotbar();
    }

    private void RefreshAll()
    {
        RefreshPlayerHotbar();
        RefreshPlayerBackpack();
        RefreshChest();
    }

    private void RefreshPlayerHotbar()
    {
        if (playerInventory == null)
        {
            return;
        }

        BindContainerToUI(
            playerInventory.Hotbar,
            hotbarSlotUIs,
            InventoryUISlotSource.Hotbar,
            true);
    }

    private void RefreshPlayerBackpack()
    {
        if (playerInventory == null)
        {
            return;
        }

        BindContainerToUI(
            playerInventory.Backpack,
            backpackSlotUIs,
            InventoryUISlotSource.Backpack,
            false);
    }

    private void RefreshChest()
    {
        InventoryContainer chestContainer = currentChest != null ? currentChest.Container : null;

        BindContainerToUI(
            chestContainer,
            chestSlotUIs,
            InventoryUISlotSource.Chest,
            false);
    }

    private void BindContainerToUI(
        InventoryContainer container,
        List<InventorySlotUI> slotUIs,
        InventoryUISlotSource source,
        bool allowSelectionHighlight)
    {
        if (slotUIs == null)
        {
            return;
        }

        for (int i = 0; i < slotUIs.Count; i++)
        {
            InventorySlotUI slotUI = slotUIs[i];
            if (slotUI == null)
            {
                continue;
            }

            InventorySlot slot = container != null && i < container.SlotCount
                ? container.GetSlot(i)
                : null;

            bool isSelected = allowSelectionHighlight &&
                              source == InventoryUISlotSource.Hotbar &&
                              playerInventory != null &&
                              i == playerInventory.SelectedHotbarIndex;

            slotUI.Bind(
                source,
                i,
                slot,
                isSelected,
                HandleSlotClicked,
                HandleBeginDrag,
                HandleDrag,
                HandleEndDrag,
                HandleDrop);
        }
    }

    private void HandleSlotClicked(InventoryUISlotSource source, int slotIndex, PointerEventData eventData)
    {
        if (playerInventory == null || currentChest == null)
        {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick(source, slotIndex);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightClick(source, slotIndex);
        }
    }

    private void HandleLeftClick(InventoryUISlotSource source, int slotIndex)
    {
        if (chestTransferController == null)
        {
            return;
        }

        if (source == InventoryUISlotSource.Hotbar)
        {
            if (!chestTransferController.MoveFromHotbarToChest(slotIndex))
            {
                playerInventory.SelectSlot(slotIndex);
            }
            return;
        }

        if (source == InventoryUISlotSource.Backpack)
        {
            chestTransferController.MoveFromBackpackToChest(slotIndex);
            return;
        }

        if (source == InventoryUISlotSource.Chest)
        {
            chestTransferController.MoveFromChestToBackpack(slotIndex);
        }
    }

    private void HandleRightClick(InventoryUISlotSource source, int slotIndex)
    {
        if (chestTransferController == null)
        {
            return;
        }

        if (source == InventoryUISlotSource.Hotbar)
        {
            chestTransferController.MoveFromHotbarToChest(slotIndex, 1);
            return;
        }

        if (source == InventoryUISlotSource.Backpack)
        {
            chestTransferController.MoveFromBackpackToChest(slotIndex, 1);
            return;
        }

        if (source == InventoryUISlotSource.Chest)
        {
            chestTransferController.MoveFromChestToBackpack(slotIndex, 1);
        }
    }

    private void HandleBeginDrag(InventoryUISlotSource source, int slotIndex, PointerEventData eventData)
    {
        InventorySlot slot = GetSlot(source, slotIndex);
        if (slot == null || slot.IsEmpty)
        {
            return;
        }

        draggingSource = source;
        draggingSlotIndex = slotIndex;

        ShowDragIcon(slot.Item != null ? slot.Item.Icon : null, eventData.position);
    }

    private void HandleDrag(PointerEventData eventData)
    {
        if (draggingSlotIndex < 0 || dragIconRect == null)
        {
            return;
        }

        dragIconRect.position = eventData.position;
    }

    private void HandleEndDrag(InventoryUISlotSource source, int slotIndex, PointerEventData eventData)
    {
        draggingSlotIndex = -1;
        HideDragIcon();
    }

    private void HandleDrop(InventoryUISlotSource targetSource, int targetSlotIndex, PointerEventData eventData)
    {
        if (draggingSlotIndex < 0)
        {
            return;
        }

        int sourceIndex = draggingSlotIndex;
        InventoryUISlotSource source = draggingSource;

        bool moved = MoveOrMergeBetweenSlots(source, sourceIndex, targetSource, targetSlotIndex);

        if (moved)
        {
            RefreshAll();
        }
    }

    private InventoryContainer GetContainer(InventoryUISlotSource source)
    {
        if (playerInventory == null)
        {
            return null;
        }

        switch (source)
        {
            case InventoryUISlotSource.Hotbar:
                return playerInventory.Hotbar;

            case InventoryUISlotSource.Backpack:
                return playerInventory.Backpack;

            case InventoryUISlotSource.Chest:
                return currentChest != null ? currentChest.Container : null;
        }

        return null;
    }

    private InventorySlot GetSlot(InventoryUISlotSource source, int slotIndex)
    {
        InventoryContainer container = GetContainer(source);
        return container != null ? container.GetSlot(slotIndex) : null;
    }

    private bool MoveOrMergeBetweenSlots(
        InventoryUISlotSource sourceType,
        int sourceIndex,
        InventoryUISlotSource targetType,
        int targetIndex)
    {
        InventoryContainer sourceContainer = GetContainer(sourceType);
        InventoryContainer targetContainer = GetContainer(targetType);

        if (sourceContainer == null || targetContainer == null)
        {
            return false;
        }

        if (sourceContainer == targetContainer && sourceIndex == targetIndex)
        {
            return false;
        }

        InventorySlot sourceSlot = sourceContainer.GetSlot(sourceIndex);
        InventorySlot targetSlot = targetContainer.GetSlot(targetIndex);

        if (sourceSlot == null || targetSlot == null || sourceSlot.IsEmpty)
        {
            return false;
        }

        if (targetSlot.IsEmpty)
        {
            targetSlot.Item = sourceSlot.Item;
            targetSlot.Amount = sourceSlot.Amount;
            sourceSlot.Clear();
            NotifyContainers(sourceContainer, targetContainer);
            return true;
        }

        if (targetSlot.Item == sourceSlot.Item && targetSlot.Item.Stackable)
        {
            int freeSpace = Mathf.Max(0, targetSlot.Item.MaxStack - targetSlot.Amount);
            if (freeSpace <= 0)
            {
                return false;
            }

            int moveAmount = Mathf.Min(freeSpace, sourceSlot.Amount);
            targetSlot.Amount += moveAmount;
            sourceSlot.Amount -= moveAmount;

            if (sourceSlot.Amount <= 0)
            {
                sourceSlot.Clear();
            }

            NotifyContainers(sourceContainer, targetContainer);
            return moveAmount > 0;
        }

        ItemData tempItem = targetSlot.Item;
        int tempAmount = targetSlot.Amount;

        targetSlot.Item = sourceSlot.Item;
        targetSlot.Amount = sourceSlot.Amount;

        sourceSlot.Item = tempItem;
        sourceSlot.Amount = tempAmount;

        NotifyContainers(sourceContainer, targetContainer);
        return true;
    }

    private void NotifyContainers(InventoryContainer a, InventoryContainer b)
    {
        a.ForceNotifyChanged();

        if (b != a)
        {
            b.ForceNotifyChanged();
        }
    }

    private void ShowDragIcon(Sprite icon, Vector2 screenPosition)
    {
        if (icon == null)
        {
            HideDragIcon();
            return;
        }

        EnsureDragIconExists();

        if (dragIconImage == null || dragIconRect == null)
        {
            return;
        }

        dragIconImage.sprite = icon;
        dragIconImage.enabled = true;
        dragIconRect.position = screenPosition;
        dragIconImage.gameObject.SetActive(true);
    }

    private void HideDragIcon()
    {
        if (dragIconImage != null)
        {
            dragIconImage.gameObject.SetActive(false);
        }
    }

    private void EnsureDragIconExists()
    {
        if (dragIconImage != null)
        {
            return;
        }

        Canvas parentCanvas = chestUIRoot != null
            ? chestUIRoot.GetComponentInParent<Canvas>()
            : GetComponentInParent<Canvas>();

        if (parentCanvas == null)
        {
            return;
        }

        GameObject dragIconObject = new GameObject("ChestDragIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        dragIconObject.transform.SetParent(parentCanvas.transform, false);

        dragIconRect = dragIconObject.GetComponent<RectTransform>();
        dragIconRect.sizeDelta = new Vector2(72f, 72f);

        CanvasGroup canvasGroup = dragIconObject.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.ignoreParentGroups = true;

        dragIconImage = dragIconObject.GetComponent<Image>();
        dragIconImage.raycastTarget = false;
        dragIconImage.preserveAspect = true;

        dragIconObject.SetActive(false);
    }
}

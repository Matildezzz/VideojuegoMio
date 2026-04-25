using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class InventoryUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject inventoryRoot;

    [Header("Hotbar UI")]
    [SerializeField] private List<InventorySlotUI> hotbarSlotUIs = new List<InventorySlotUI>();

    [Header("Backpack UI")]
    [SerializeField] private List<InventorySlotUI> backpackSlotUIs = new List<InventorySlotUI>();

    private InventoryUISlotSource draggingSource;
    private int draggingSlotIndex = -1;
    private Image dragIconImage;
    private RectTransform dragIconRect;

    public bool IsOpen => inventoryRoot != null && inventoryRoot.activeSelf;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshAll();
    }

    private void OnDisable()
    {
        Unsubscribe();
        HideDragIcon();
        draggingSlotIndex = -1;
    }

    public void OpenInventory()
    {
        if (inventoryRoot != null)
        {
            inventoryRoot.SetActive(true);
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.NotifyInventoryOpened();
        }

        RefreshAll();
    }

    public void CloseInventory()
    {
        if (inventoryRoot != null)
        {
            inventoryRoot.SetActive(false);
        }

        HideDragIcon();
        draggingSlotIndex = -1;
    }

    public void ToggleInventory()
    {
        if (inventoryRoot == null)
        {
            return;
        }

        inventoryRoot.SetActive(!inventoryRoot.activeSelf);

        if (inventoryRoot.activeSelf)
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.NotifyInventoryOpened();
            }

            RefreshAll();
        }
        else
        {
            HideDragIcon();
            draggingSlotIndex = -1;
        }
    }

    private void Subscribe()
    {
        if (playerInventory == null)
        {
            return;
        }

        playerInventory.OnInventoryChanged -= HandleInventoryChanged;
        playerInventory.OnInventoryChanged += HandleInventoryChanged;

        playerInventory.OnSelectedHotbarChanged -= HandleSelectedHotbarChanged;
        playerInventory.OnSelectedHotbarChanged += HandleSelectedHotbarChanged;
    }

    private void Unsubscribe()
    {
        if (playerInventory == null)
        {
            return;
        }

        playerInventory.OnInventoryChanged -= HandleInventoryChanged;
        playerInventory.OnSelectedHotbarChanged -= HandleSelectedHotbarChanged;
    }

    private void HandleInventoryChanged()
    {
        RefreshAll();
    }

    private void HandleSelectedHotbarChanged(int selectedIndex)
    {
        RefreshHotbar();
    }

    private void RefreshAll()
    {
        RefreshHotbar();
        RefreshBackpack();
    }

    private void RefreshHotbar()
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

    private void RefreshBackpack()
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

    private void BindContainerToUI(
        InventoryContainer container,
        List<InventorySlotUI> slotUIs,
        InventoryUISlotSource source,
        bool allowSelectionHighlight)
    {
        if (container == null || slotUIs == null)
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

            InventorySlot slot = i < container.SlotCount ? container.GetSlot(i) : null;

            bool isSelected = allowSelectionHighlight &&
                              source == InventoryUISlotSource.Hotbar &&
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
        if (playerInventory == null)
        {
            return;
        }

        if (source == InventoryUISlotSource.Hotbar)
        {
            HandleHotbarClick(slotIndex, eventData);
            return;
        }

        if (source == InventoryUISlotSource.Backpack)
        {
            HandleBackpackClick(slotIndex, eventData);
        }
    }

    private void HandleHotbarClick(int slotIndex, PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (InventoryInputState.IsQuickMoveModifierPressed())
            {
                playerInventory.MoveHotbarToBackpack(slotIndex);
                return;
            }

            playerInventory.SelectSlot(slotIndex);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            playerInventory.SelectSlot(slotIndex);
            playerInventory.UseSelectedItem();
        }
    }

    private void HandleBackpackClick(int slotIndex, PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (InventoryInputState.IsQuickMoveModifierPressed())
            {
                playerInventory.MoveBackpackToHotbar(slotIndex);
            }
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

        Canvas parentCanvas = inventoryRoot != null
            ? inventoryRoot.GetComponentInParent<Canvas>()
            : GetComponentInParent<Canvas>();

        if (parentCanvas == null)
        {
            return;
        }

        GameObject dragIconObject = new GameObject(
            "InventoryDragIcon",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(Image));

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
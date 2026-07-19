using System.Collections;
using TMPro;
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
    [SerializeField]
    private List<InventorySlotUI> hotbarSlotUIs =
        new List<InventorySlotUI>();

    [Header("Backpack UI")]
    [SerializeField]
    private List<InventorySlotUI> backpackSlotUIs =
        new List<InventorySlotUI>();

    [Header("Preview")]
    [SerializeField] private bool previewHotbarSlotOnHover = true;

    [Header("Item Name")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private float hoverNameDelay = 2f;
    [SerializeField] private float itemNameDuration = 2f;

    private Coroutine hoverNameCoroutine;
    private Coroutine hideNameCoroutine;

    private int previewHotbarIndex = -1;

    private InventoryUISlotSource draggingSource;
    private int draggingSlotIndex = -1;

    private Image dragIconImage;
    private RectTransform dragIconRect;

    public bool IsOpen =>
        inventoryRoot != null &&
        inventoryRoot.activeSelf;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory =
                FindFirstObjectByType<PlayerInventory>();
        }
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshAll();
        ShowSelectedItemName();
    }

    private void OnDisable()
    {
        Unsubscribe();

        CancelHoverNamePreview();
        CancelHideNameTimer();

        HideDragIcon();
        HideItemName();

        previewHotbarIndex = -1;
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

        previewHotbarIndex = -1;
        draggingSlotIndex = -1;

        RefreshHotbar();
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

            previewHotbarIndex = -1;
            draggingSlotIndex = -1;

            RefreshHotbar();
        }
    }

    private void Subscribe()
    {
        if (playerInventory == null)
        {
            return;
        }

        playerInventory.OnInventoryChanged -=
            HandleInventoryChanged;

        playerInventory.OnInventoryChanged +=
            HandleInventoryChanged;

        playerInventory.OnSelectedHotbarChanged -=
            HandleSelectedHotbarChanged;

        playerInventory.OnSelectedHotbarChanged +=
            HandleSelectedHotbarChanged;
    }

    private void Unsubscribe()
    {
        if (playerInventory == null)
        {
            return;
        }

        playerInventory.OnInventoryChanged -=
            HandleInventoryChanged;

        playerInventory.OnSelectedHotbarChanged -=
            HandleSelectedHotbarChanged;
    }

    private void HandleInventoryChanged()
    {
        RefreshAll();
    }

    private void HandleSelectedHotbarChanged(int selectedIndex)
    {
        CancelHoverNamePreview();
        RefreshHotbar();
        ShowSelectedItemName();
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
            true
        );
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
            false
        );
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

            InventorySlot slot =
                i < container.SlotCount
                    ? container.GetSlot(i)
                    : null;

            bool isSelected =
                allowSelectionHighlight &&
                source == InventoryUISlotSource.Hotbar &&
                i == playerInventory.SelectedHotbarIndex;

            bool isPreviewed =
                previewHotbarSlotOnHover &&
                source == InventoryUISlotSource.Hotbar &&
                i == previewHotbarIndex;

            slotUI.Bind(
                source,
                i,
                slot,
                isSelected,
                isPreviewed,
                HandleSlotClicked,
                HandleSlotHoverEnter,
                HandleSlotHoverExit,
                HandleBeginDrag,
                HandleDrag,
                HandleEndDrag,
                HandleDrop
            );
        }
    }

    private void HandleSlotClicked(
        InventoryUISlotSource source,
        int slotIndex,
        PointerEventData eventData)
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

    private void HandleSlotHoverEnter(
        InventoryUISlotSource source,
        int slotIndex,
        PointerEventData eventData)
    {
        if (!previewHotbarSlotOnHover)
        {
            return;
        }

        if (source != InventoryUISlotSource.Hotbar)
        {
            return;
        }

        if (playerInventory == null ||
            playerInventory.Hotbar == null)
        {
            return;
        }

        if (slotIndex < 0 ||
            slotIndex >= playerInventory.Hotbar.SlotCount)
        {
            return;
        }

        previewHotbarIndex = slotIndex;
        RefreshHotbar();

        StartHoverNamePreview(slotIndex);
    }

    private void HandleSlotHoverExit(
        InventoryUISlotSource source,
        int slotIndex,
        PointerEventData eventData)
    {
        if (source != InventoryUISlotSource.Hotbar)
        {
            return;
        }

        if (previewHotbarIndex != slotIndex)
        {
            return;
        }

        previewHotbarIndex = -1;

        CancelHoverNamePreview();
        CancelHideNameTimer();

        HideItemName();
        RefreshHotbar();
    }

    private void HandleHotbarClick(
        int slotIndex,
        PointerEventData eventData)
    {
        if (eventData.button ==
            PointerEventData.InputButton.Left)
        {
            if (InventoryInputState
                .IsQuickMoveModifierPressed())
            {
                playerInventory.MoveHotbarToBackpack(
                    slotIndex
                );

                return;
            }

            playerInventory.SelectSlot(slotIndex);
            return;
        }

        if (eventData.button ==
            PointerEventData.InputButton.Right)
        {
            playerInventory.SelectSlot(slotIndex);
            playerInventory.UseSelectedItem();
        }
    }

    private void HandleBackpackClick(
        int slotIndex,
        PointerEventData eventData)
    {
        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }

        if (InventoryInputState
            .IsQuickMoveModifierPressed())
        {
            playerInventory.MoveBackpackToHotbar(slotIndex);
        }
    }

    private void HandleBeginDrag(
        InventoryUISlotSource source,
        int slotIndex,
        PointerEventData eventData)
    {
        InventorySlot slot = GetSlot(source, slotIndex);

        if (slot == null || slot.IsEmpty)
        {
            return;
        }

        draggingSource = source;
        draggingSlotIndex = slotIndex;

        Sprite icon = slot.Item != null
            ? slot.Item.Icon
            : null;

        ShowDragIcon(icon, eventData.position);
    }

    private void HandleDrag(PointerEventData eventData)
    {
        if (draggingSlotIndex < 0 ||
            dragIconRect == null)
        {
            return;
        }

        dragIconRect.position = eventData.position;
    }

    private void HandleEndDrag(
        InventoryUISlotSource source,
        int slotIndex,
        PointerEventData eventData)
    {
        draggingSlotIndex = -1;
        HideDragIcon();
    }

    private void HandleDrop(
        InventoryUISlotSource targetSource,
        int targetSlotIndex,
        PointerEventData eventData)
    {
        if (draggingSlotIndex < 0)
        {
            return;
        }

        int sourceIndex = draggingSlotIndex;
        InventoryUISlotSource source = draggingSource;

        bool moved = MoveOrMergeBetweenSlots(
            source,
            sourceIndex,
            targetSource,
            targetSlotIndex
        );

        if (moved)
        {
            RefreshAll();
        }
    }

    private InventoryContainer GetContainer(
        InventoryUISlotSource source)
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

            default:
                return null;
        }
    }

    private InventorySlot GetSlot(
        InventoryUISlotSource source,
        int slotIndex)
    {
        InventoryContainer container =
            GetContainer(source);

        if (container == null)
        {
            return null;
        }

        if (slotIndex < 0 ||
            slotIndex >= container.SlotCount)
        {
            return null;
        }

        return container.GetSlot(slotIndex);
    }

    private bool MoveOrMergeBetweenSlots(
        InventoryUISlotSource sourceType,
        int sourceIndex,
        InventoryUISlotSource targetType,
        int targetIndex)
    {
        InventoryContainer sourceContainer =
            GetContainer(sourceType);

        InventoryContainer targetContainer =
            GetContainer(targetType);

        if (sourceContainer == null ||
            targetContainer == null)
        {
            return false;
        }

        if (sourceIndex < 0 ||
            sourceIndex >= sourceContainer.SlotCount)
        {
            return false;
        }

        if (targetIndex < 0 ||
            targetIndex >= targetContainer.SlotCount)
        {
            return false;
        }

        if (sourceContainer == targetContainer &&
            sourceIndex == targetIndex)
        {
            return false;
        }

        InventorySlot sourceSlot =
            sourceContainer.GetSlot(sourceIndex);

        InventorySlot targetSlot =
            targetContainer.GetSlot(targetIndex);

        if (sourceSlot == null ||
            targetSlot == null ||
            sourceSlot.IsEmpty)
        {
            return false;
        }

        if (targetSlot.IsEmpty)
        {
            targetSlot.Item = sourceSlot.Item;
            targetSlot.Amount = sourceSlot.Amount;

            sourceSlot.Clear();

            NotifyContainers(
                sourceContainer,
                targetContainer
            );

            return true;
        }

        if (targetSlot.Item == sourceSlot.Item &&
            targetSlot.Item.Stackable)
        {
            int freeSpace = Mathf.Max(
                0,
                targetSlot.Item.MaxStack -
                targetSlot.Amount
            );

            if (freeSpace <= 0)
            {
                return false;
            }

            int moveAmount = Mathf.Min(
                freeSpace,
                sourceSlot.Amount
            );

            targetSlot.Amount += moveAmount;
            sourceSlot.Amount -= moveAmount;

            if (sourceSlot.Amount <= 0)
            {
                sourceSlot.Clear();
            }

            NotifyContainers(
                sourceContainer,
                targetContainer
            );

            return moveAmount > 0;
        }

        ItemData temporaryItem = targetSlot.Item;
        int temporaryAmount = targetSlot.Amount;

        targetSlot.Item = sourceSlot.Item;
        targetSlot.Amount = sourceSlot.Amount;

        sourceSlot.Item = temporaryItem;
        sourceSlot.Amount = temporaryAmount;

        NotifyContainers(
            sourceContainer,
            targetContainer
        );

        return true;
    }

    private void NotifyContainers(
        InventoryContainer first,
        InventoryContainer second)
    {
        first.ForceNotifyChanged();

        if (second != first)
        {
            second.ForceNotifyChanged();
        }
    }

    private void ShowDragIcon(
        Sprite icon,
        Vector2 screenPosition)
    {
        if (icon == null)
        {
            HideDragIcon();
            return;
        }

        EnsureDragIconExists();

        if (dragIconImage == null ||
            dragIconRect == null)
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
            typeof(Image)
        );

        dragIconObject.transform.SetParent(
            parentCanvas.transform,
            false
        );

        dragIconRect =
            dragIconObject.GetComponent<RectTransform>();

        dragIconRect.sizeDelta =
            new Vector2(72f, 72f);

        CanvasGroup canvasGroup =
            dragIconObject.GetComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.ignoreParentGroups = true;

        dragIconImage =
            dragIconObject.GetComponent<Image>();

        dragIconImage.raycastTarget = false;
        dragIconImage.preserveAspect = true;

        dragIconObject.SetActive(false);
    }

    private void StartHoverNamePreview(int slotIndex)
    {
        CancelHoverNamePreview();

        InventorySlot slot = GetSlot(
            InventoryUISlotSource.Hotbar,
            slotIndex
        );

        if (slot == null ||
            slot.IsEmpty ||
            slot.Item == null)
        {
            return;
        }

        hoverNameCoroutine = StartCoroutine(
            ShowHoveredItemNameAfterDelay(
                slotIndex,
                slot.Item
            )
        );
    }

    private IEnumerator ShowHoveredItemNameAfterDelay(
        int slotIndex,
        ItemData item)
    {
        yield return new WaitForSecondsRealtime(
            hoverNameDelay
        );

        if (previewHotbarIndex != slotIndex)
        {
            hoverNameCoroutine = null;
            yield break;
        }

        hoverNameCoroutine = null;

        ShowItemName(
            item,
            false
        );
    }

    private void CancelHoverNamePreview()
    {
        if (hoverNameCoroutine == null)
        {
            return;
        }

        StopCoroutine(hoverNameCoroutine);
        hoverNameCoroutine = null;
    }

    private void ShowSelectedItemName()
    {
        if (playerInventory == null)
        {
            HideItemName();
            return;
        }

        InventorySlot selectedSlot =
            playerInventory.GetSelectedSlot();

        if (selectedSlot == null ||
            selectedSlot.IsEmpty ||
            selectedSlot.Item == null)
        {
            HideItemName();
            return;
        }

        ShowItemName(
            selectedSlot.Item,
            true
        );
    }

        private void ShowItemName(
        ItemData item,
        bool hideAutomatically)
    {
        if (itemNameText == null || item == null)
        {
            HideItemName();
            return;
        }

        CancelHideNameTimer();

        itemNameText.text = item.DisplayName;
        itemNameText.gameObject.SetActive(true);

        if (hideAutomatically)
        {
            RestartHideNameTimer();
        }
    }

    private void HideItemName()
    {
        if (itemNameText == null)
        {
            return;
        }

        itemNameText.text = string.Empty;
        itemNameText.gameObject.SetActive(false);
    }

    private void RestartHideNameTimer()
    {
        CancelHideNameTimer();

        hideNameCoroutine = StartCoroutine(
            HideItemNameAfterDelay()
        );
    }

    private IEnumerator HideItemNameAfterDelay()
    {
        yield return new WaitForSecondsRealtime(
            itemNameDuration
        );

        hideNameCoroutine = null;
        HideItemName();
    }

    private void CancelHideNameTimer()
    {
        if (hideNameCoroutine == null)
        {
            return;
        }

        StopCoroutine(hideNameCoroutine);
        hideNameCoroutine = null;
    }
}
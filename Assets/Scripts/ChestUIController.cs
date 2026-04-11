using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

        if (IsOpen)
        {
            PauseController.SetPause(false);
        }
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

            slotUI.Bind(source, i, slot, isSelected, HandleSlotClicked);
        }
    }

    private void HandleSlotClicked(InventoryUISlotSource source, int slotIndex, PointerEventData eventData)
    {
        if (playerInventory == null)
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
        bool quickMove = InventoryInputState.IsQuickMoveModifierPressed();

        if (source == InventoryUISlotSource.Hotbar)
        {
            if (quickMove)
            {
                if (chestTransferController != null)
                {
                    chestTransferController.MoveFromHotbarToChest(slotIndex);
                }

                return;
            }

            playerInventory.SelectSlot(slotIndex);
            return;
        }

        if (source == InventoryUISlotSource.Backpack)
        {
            if (quickMove)
            {
                if (chestTransferController != null)
                {
                    chestTransferController.MoveFromBackpackToChest(slotIndex);
                }
            }

            return;
        }

        if (source == InventoryUISlotSource.Chest)
        {
            if (quickMove)
            {
                if (chestTransferController != null)
                {
                    chestTransferController.MoveFromChestToBackpack(slotIndex);
                }
            }
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
}
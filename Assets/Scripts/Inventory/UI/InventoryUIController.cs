using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InventoryUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject inventoryRoot;

    [Header("Hotbar UI")]
    [SerializeField] private List<InventorySlotUI> hotbarSlotUIs = new List<InventorySlotUI>();

    [Header("Backpack UI")]
    [SerializeField] private List<InventorySlotUI> backpackSlotUIs = new List<InventorySlotUI>();

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
    }

    public void OpenInventory()
    {
        if (inventoryRoot != null)
        {
            inventoryRoot.SetActive(true);
        }

        RefreshAll();
    }

    public void CloseInventory()
    {
        if (inventoryRoot != null)
        {
            inventoryRoot.SetActive(false);
        }
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
            RefreshAll();
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

            slotUI.Bind(source, i, slot, isSelected, HandleSlotClicked);
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
}
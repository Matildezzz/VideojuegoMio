using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private GameObject selectedHighlight;
    [SerializeField] private GameObject emptyStateRoot;

    private InventoryUISlotSource slotSource;
    private int slotIndex;
    private Action<InventoryUISlotSource, int, PointerEventData> clickCallback;

    public void Bind(
        InventoryUISlotSource source,
        int index,
        InventorySlot slot,
        bool isSelected,
        Action<InventoryUISlotSource, int, PointerEventData> onClick)
    {
        slotSource = source;
        slotIndex = index;
        clickCallback = onClick;

        Refresh(slot, isSelected);
    }

    public void Refresh(InventorySlot slot, bool isSelected)
    {
        bool hasItem = slot != null && !slot.IsEmpty;

        if (iconImage != null)
        {
            iconImage.enabled = hasItem;
            iconImage.sprite = hasItem ? slot.Item.Icon : null;
        }

        if (amountText != null)
        {
            amountText.text = hasItem ? slot.Amount.ToString() : string.Empty;
        }

        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(isSelected);
        }

        if (emptyStateRoot != null)
        {
            emptyStateRoot.SetActive(!hasItem);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clickCallback?.Invoke(slotSource, slotIndex, eventData);
    }
}
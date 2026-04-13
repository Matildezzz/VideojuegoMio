using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [Header("UI")]
    [SerializeField] private Image slotBackgroundImage;
    [SerializeField] private Sprite normalBackgroundSprite;
    [SerializeField] private Sprite selectedBackgroundSprite;

    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private GameObject selectedHighlight;
    [SerializeField] private GameObject emptyStateRoot;

    private InventoryUISlotSource slotSource;
    private int slotIndex;

    private Action<InventoryUISlotSource, int, PointerEventData> clickCallback;
    private Action<InventoryUISlotSource, int, PointerEventData> beginDragCallback;
    private Action<PointerEventData> dragCallback;
    private Action<InventoryUISlotSource, int, PointerEventData> endDragCallback;
    private Action<InventoryUISlotSource, int, PointerEventData> dropCallback;

    public void Bind(
        InventoryUISlotSource source,
        int index,
        InventorySlot slot,
        bool isSelected,
        Action<InventoryUISlotSource, int, PointerEventData> onClick,
        Action<InventoryUISlotSource, int, PointerEventData> onBeginDrag = null,
        Action<PointerEventData> onDrag = null,
        Action<InventoryUISlotSource, int, PointerEventData> onEndDrag = null,
        Action<InventoryUISlotSource, int, PointerEventData> onDrop = null)
    {
        slotSource = source;
        slotIndex = index;

        clickCallback = onClick;
        beginDragCallback = onBeginDrag;
        dragCallback = onDrag;
        endDragCallback = onEndDrag;
        dropCallback = onDrop;

        Refresh(slot, isSelected);
    }

    public void Refresh(InventorySlot slot, bool isSelected)
    {
        bool hasItem = slot != null && !slot.IsEmpty;

        if (iconImage != null)
        {
            iconImage.enabled = hasItem;
            iconImage.sprite = hasItem ? slot.Item.Icon : null;
            iconImage.preserveAspect = true;
        }

        if (amountText != null)
        {
            amountText.text = hasItem ? slot.Amount.ToString() : string.Empty;
        }

        if (slotBackgroundImage != null)
        {
            if (isSelected && selectedBackgroundSprite != null)
            {
                slotBackgroundImage.sprite = selectedBackgroundSprite;
            }
            else if (normalBackgroundSprite != null)
            {
                slotBackgroundImage.sprite = normalBackgroundSprite;
            }
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        beginDragCallback?.Invoke(slotSource, slotIndex, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragCallback?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        endDragCallback?.Invoke(slotSource, slotIndex, eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        dropCallback?.Invoke(slotSource, slotIndex, eventData);
    }
}

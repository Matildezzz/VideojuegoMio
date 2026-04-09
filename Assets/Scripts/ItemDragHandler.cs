using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private Slot originalSlot;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool droppedOnSlot;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSlot = originalParent.GetComponent<Slot>();
        droppedOnSlot = false;

        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (!droppedOnSlot)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    public void DropOnSlot(Slot dropSlot)
    {
        if (dropSlot == null || originalSlot == null)
        {
            ReturnToOriginalSlot();
            return;
        }

        InventoryItemUI draggedItem = GetComponent<InventoryItemUI>();
        if (draggedItem == null)
        {
            ReturnToOriginalSlot();
            return;
        }

        if (dropSlot == originalSlot)
        {
            ReturnToOriginalSlot();
            droppedOnSlot = true;
            return;
        }

        if (dropSlot.currentItem != null)
        {
            InventoryItemUI targetItem = dropSlot.currentItem.GetComponent<InventoryItemUI>();

            if (targetItem != null && targetItem.itemData == draggedItem.itemData)
            {
                targetItem.AddToStack(draggedItem.quantity);
                originalSlot.currentItem = null;
                droppedOnSlot = true;
                Destroy(gameObject);
                InventoryController.Instance.RebuildItemCounts();
                return;
            }

            GameObject targetObject = dropSlot.currentItem;

            targetObject.transform.SetParent(originalSlot.transform);
            targetObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            originalSlot.currentItem = targetObject;

            transform.SetParent(dropSlot.transform);
            rectTransform.anchoredPosition = Vector2.zero;
            dropSlot.currentItem = gameObject;

            droppedOnSlot = true;
            InventoryController.Instance.RebuildItemCounts();
            return;
        }

        originalSlot.currentItem = null;
        transform.SetParent(dropSlot.transform);
        rectTransform.anchoredPosition = Vector2.zero;
        dropSlot.currentItem = gameObject;

        droppedOnSlot = true;
        InventoryController.Instance.RebuildItemCounts();
    }

    private void ReturnToOriginalSlot()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}
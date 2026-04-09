using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    public GameObject currentItem;

    [HideInInspector] public MonoBehaviour owner;
    [HideInInspector] public int slotIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ChestInteraction.CurrentOpenChest == null)
        {
            return;
        }

        ChestInteraction.CurrentOpenChest.HandleSlotClick(this, eventData.button);
    }

    public void OnDrop(PointerEventData eventData)
    {
        ItemDragHandler dragHandler = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<ItemDragHandler>()
            : null;

        if (dragHandler != null)
        {
            dragHandler.DropOnSlot(this);
        }
    }
}
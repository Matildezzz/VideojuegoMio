using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerClickHandler
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
}
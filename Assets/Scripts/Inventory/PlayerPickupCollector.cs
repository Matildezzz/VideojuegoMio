using UnityEngine;

public sealed class PlayerPickupCollector : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerInventory == null)
        {
            return;
        }

        DroppedItem droppedItem = collision.GetComponent<DroppedItem>();
        if (droppedItem != null)
        {
            droppedItem.TryPickup(playerInventory);
            return;
        }

        ItemPickup itemPickup = collision.GetComponent<ItemPickup>();
        if (itemPickup != null)
        {
            itemPickup.TryPickup(playerInventory);
        }
    }
}

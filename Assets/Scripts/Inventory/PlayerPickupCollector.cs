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
            return;
        }

        WorldItem worldItem = collision.GetComponent<WorldItem>();
        if (worldItem != null && worldItem.itemData != null && worldItem.quantity > 0)
        {
            bool pickedUp = playerInventory.TryAddItem(worldItem.itemData, worldItem.quantity);

            if (pickedUp)
            {
                worldItem.ShowPopUp();
                Destroy(worldItem.gameObject);
            }
        }
    }
}
using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private HotbarController hotbarController;
    private InventoryController inventoryController;

    private void Start()
    {
        hotbarController = FindFirstObjectByType<HotbarController>();
        inventoryController = FindFirstObjectByType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Item"))
        {
            return;
        }

        WorldItem worldItem = collision.GetComponent<WorldItem>();

        if (worldItem == null || worldItem.itemData == null)
        {
            Debug.Log("No se encontró WorldItem o ItemData en " + collision.name);
            return;
        }

        bool itemAdded = false;

        if (hotbarController != null)
        {
            itemAdded = hotbarController.AddItemByData(worldItem.itemData, worldItem.quantity);
        }

        if (!itemAdded && inventoryController != null)
        {
            itemAdded = inventoryController.AddItemByData(worldItem.itemData, worldItem.quantity);
        }

        if (itemAdded)
        {
            worldItem.ShowPopUp();
            Destroy(collision.gameObject);
        }
    }
}
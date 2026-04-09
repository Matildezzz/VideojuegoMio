using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;

    private void Start()
    {
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

        bool itemAdded = inventoryController.AddItemByData(worldItem.itemData, worldItem.quantity);

        if (itemAdded)
        {
            worldItem.ShowPopUp();
            Destroy(collision.gameObject);
        }
    }
}
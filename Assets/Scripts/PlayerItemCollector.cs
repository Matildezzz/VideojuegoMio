using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;

    void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Item"))
        {
            return;
        }

        Item item = collision.GetComponent<Item>();

        if (item == null)
        {
            return;
        }

        bool itemAdded = inventoryController.AddItem(item.GetInventoryPrefab(), item.quantity);

        if (itemAdded)
        {
            item.ShowPopUp();
            Destroy(collision.gameObject);
        }
    }
}
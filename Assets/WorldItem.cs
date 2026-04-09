using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemData itemData;
    public int quantity = 1;

    public SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void ShowPopUp()
    {
        if (ItemPickupUIController.Instance != null && itemData != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(itemData.itemName, itemData.inventoryIcon);
        }
    }
}
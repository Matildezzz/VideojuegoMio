using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int quantity = 1;

    [Header("Prefabs")]
    public GameObject inventoryPrefab;
    public GameObject worldPrefab;

    [Header("Visual")]
    public Sprite inventoryIcon;

    private TMP_Text quantityText;
    private Image itemImage;
    private SpriteRenderer spriteRenderer;

    public int buyPrice = 10;

    [Range(0, 1)]
    public float sellPriceMultiplier = 0.5f;

    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>(true);
        itemImage = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateQuantityDisplay();
    }

    public GameObject GetInventoryPrefab()
    {
        if (inventoryPrefab != null)
        {
            return inventoryPrefab;
        }

        return gameObject;
    }

    public GameObject GetWorldPrefab()
    {
        if (worldPrefab != null)
        {
            return worldPrefab;
        }

        return gameObject;
    }

    public Sprite GetDisplayIcon()
    {
        if (inventoryIcon != null)
        {
            return inventoryIcon;
        }

        if (itemImage != null && itemImage.sprite != null)
        {
            return itemImage.sprite;
        }

        if (spriteRenderer != null)
        {
            return spriteRenderer.sprite;
        }

        return null;
    }

    public int GetSellPrice()
    {
        return Mathf.RoundToInt(buyPrice * sellPriceMultiplier);
    }

    public void UpdateQuantityDisplay()
    {
        if (quantityText != null)
        {
            quantityText.text = quantity.ToString();
        }
    }

    public void AddToStack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantityDisplay();
    }

    public int RemoveFromStack(int amount = 1)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        UpdateQuantityDisplay();
        return removed;
    }

    public GameObject CloneItem(int newQuantity)
    {
        GameObject clone = Instantiate(gameObject);
        Item cloneItem = clone.GetComponent<Item>();
        cloneItem.quantity = newQuantity;
        cloneItem.UpdateQuantityDisplay();
        return clone;
    }

    public virtual void UseItem()
    {
        Debug.Log("Using item " + Name);
    }

    public virtual void ShowPopUp()
    {
        if (ItemPickupUIController.Instance != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(Name, GetDisplayIcon());
        }
    }
}
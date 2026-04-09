using System;
using TMPro;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int quantity = 1;

    private TMP_Text quantityText;

    // Shop fields
    public int buyPrice = 10; // Buy from shop
    [Range(0,1)]
    public float sellPriceMultiplier = 0.5f; // Sell for 50% buy price

    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();
        UpdateQuantityDisplay();
    }

    public int GetSellPrice()
    {
        return Mathf.RoundToInt(buyPrice * sellPriceMultiplier);
    }

    public void UpdateQuantityDisplay()
{
    if(quantityText != null)
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
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Sprite itemIcon = sr != null ? sr.sprite : null;

        if (ItemPickupUIController.Instance != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
        }
    }
}
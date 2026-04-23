using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    public string shopID = "shop_merchan_01";
    public string shopkeeperName = "Merchant";

    public List<ShopStockItem> defaultShopStock = new();
    private List<ShopStockItem> currentShopStock = new();

    private bool isInitialized = false;

    [System.Serializable]
    public class ShopStockItem
    {
        public string itemId;
        public int quantity;
    }

    private void Start()
    {
        InitializeShop();
    }

    private void InitializeShop()
    {
        if (isInitialized)
        {
            return;
        }

        currentShopStock = new List<ShopStockItem>();

        foreach (var item in defaultShopStock)
        {
            currentShopStock.Add(new ShopStockItem
            {
                itemId = item.itemId,
                quantity = item.quantity
            });
        }

        isInitialized = true;
    }

    public List<ShopStockItem> GetCurrentStock()
    {
        return currentShopStock;
    }

    public void SetStock(List<ShopStockItem> stock)
    {
        currentShopStock = stock ?? new List<ShopStockItem>();
    }

    public void AddToStock(string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return;
        }

        ShopStockItem existing = currentShopStock.Find(s => s.itemId == itemId);

        if (existing != null)
        {
            existing.quantity += quantity;
        }
        else
        {
            currentShopStock.Add(new ShopStockItem
            {
                itemId = itemId,
                quantity = quantity
            });
        }
    }

    public bool RemoveFromShopStock(string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return false;
        }

        ShopStockItem existing = currentShopStock.Find(s => s.itemId == itemId);

        if (existing == null || existing.quantity < quantity)
        {
            return false;
        }

        existing.quantity -= quantity;

        if (existing.quantity <= 0)
        {
            currentShopStock.Remove(existing);
        }

        return true;
    }
}
using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public ItemData Item;
    public int Amount;

    public bool IsEmpty => Item == null || Amount <= 0;

    public int FreeSpace
    {
        get
        {
            if (IsEmpty)
            {
                return 0;
            }

            if (!Item.Stackable)
            {
                return 0;
            }

            return Mathf.Max(0, Item.MaxStack - Amount);
        }
    }

    public bool CanStack(ItemData other)
    {
        return !IsEmpty &&
               Item == other &&
               Item.Stackable &&
               Amount < Item.MaxStack;
    }

    public void Set(ItemData item, int amount)
    {
        Item = item;
        Amount = Mathf.Max(0, amount);

        if (Amount <= 0)
        {
            Clear();
        }
    }

    public void Clear()
    {
        Item = null;
        Amount = 0;
    }
}
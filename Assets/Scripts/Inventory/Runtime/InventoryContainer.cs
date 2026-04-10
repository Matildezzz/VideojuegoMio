using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryContainer
{
    public event Action OnChanged;

    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    public int SlotCount => slots.Count;

    public InventoryContainer()
    {
    }

    public InventoryContainer(int slotCount)
    {
        Initialize(slotCount);
    }

    public void Initialize(int slotCount)
    {
        slotCount = Mathf.Max(0, slotCount);

        if (slots == null)
        {
            slots = new List<InventorySlot>();
        }

        slots.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            slots.Add(new InventorySlot());
        }

        NotifyChanged();
    }

    public InventorySlot GetSlot(int index)
    {
        if (!IsValidIndex(index))
        {
            return null;
        }

        return slots[index];
    }

    public bool CanAddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        int remaining = amount;

        if (item.Stackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = slots[i];

                if (!slot.CanStack(item))
                {
                    continue;
                }

                int canAdd = Mathf.Min(slot.FreeSpace, remaining);
                remaining -= canAdd;

                if (remaining <= 0)
                {
                    return true;
                }
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (!slot.IsEmpty)
            {
                continue;
            }

            int spaceInEmptySlot = item.Stackable ? item.MaxStack : 1;
            int canAdd = Mathf.Min(spaceInEmptySlot, remaining);
            remaining -= canAdd;

            if (remaining <= 0)
            {
                return true;
            }
        }

        return remaining <= 0;
    }

    public int AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return amount;
        }

        int remaining = amount;
        bool changed = false;

        if (item.Stackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = slots[i];

                if (!slot.CanStack(item))
                {
                    continue;
                }

                int addAmount = Mathf.Min(slot.FreeSpace, remaining);
                if (addAmount <= 0)
                {
                    continue;
                }

                slot.Amount += addAmount;
                remaining -= addAmount;
                changed = true;

                if (remaining <= 0)
                {
                    break;
                }
            }
        }

        if (remaining > 0)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = slots[i];

                if (!slot.IsEmpty)
                {
                    continue;
                }

                int addAmount = item.Stackable
                    ? Mathf.Min(item.MaxStack, remaining)
                    : 1;

                slot.Item = item;
                slot.Amount = addAmount;

                remaining -= addAmount;
                changed = true;

                if (remaining <= 0)
                {
                    break;
                }
            }
        }

        if (changed)
        {
            NotifyChanged();
        }

        return remaining;
    }

    public bool RemoveItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        int total = CountItem(item);
        if (total < amount)
        {
            return false;
        }

        int remaining = amount;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (slot.IsEmpty || slot.Item != item)
            {
                continue;
            }

            int removeAmount = Mathf.Min(slot.Amount, remaining);
            slot.Amount -= removeAmount;
            remaining -= removeAmount;

            if (slot.Amount <= 0)
            {
                slot.Clear();
            }

            if (remaining <= 0)
            {
                break;
            }
        }

        NotifyChanged();
        return true;
    }

    public bool RemoveFromSlot(int slotIndex, int amount)
    {
        if (!IsValidIndex(slotIndex) || amount <= 0)
        {
            return false;
        }

        InventorySlot slot = slots[slotIndex];

        if (slot.IsEmpty)
        {
            return false;
        }

        int removeAmount = Mathf.Min(slot.Amount, amount);
        slot.Amount -= removeAmount;

        if (slot.Amount <= 0)
        {
            slot.Clear();
        }

        NotifyChanged();
        return true;
    }

    public void SwapSlots(int a, int b)
    {
        if (!IsValidIndex(a) || !IsValidIndex(b) || a == b)
        {
            return;
        }

        InventorySlot temp = slots[a];
        slots[a] = slots[b];
        slots[b] = temp;

        NotifyChanged();
    }

    public bool MoveSlotTo(InventoryContainer other, int fromIndex, int amount)
    {
        if (other == null || other == this)
        {
            return false;
        }

        if (!IsValidIndex(fromIndex))
        {
            return false;
        }

        InventorySlot sourceSlot = slots[fromIndex];
        if (sourceSlot.IsEmpty)
        {
            return false;
        }

        int amountToMove = amount <= 0
            ? sourceSlot.Amount
            : Mathf.Min(amount, sourceSlot.Amount);

        int remaining = other.AddItem(sourceSlot.Item, amountToMove);
        int moved = amountToMove - remaining;

        if (moved <= 0)
        {
            return false;
        }

        sourceSlot.Amount -= moved;

        if (sourceSlot.Amount <= 0)
        {
            sourceSlot.Clear();
        }

        NotifyChanged();
        return true;
    }

    public bool SplitStack(int fromIndex, int toIndex, int amount)
    {
        if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex) || fromIndex == toIndex)
        {
            return false;
        }

        InventorySlot fromSlot = slots[fromIndex];
        InventorySlot toSlot = slots[toIndex];

        if (fromSlot.IsEmpty)
        {
            return false;
        }

        if (!fromSlot.Item.Stackable)
        {
            return false;
        }

        if (amount <= 0 || amount >= fromSlot.Amount)
        {
            return false;
        }

        if (toSlot.IsEmpty)
        {
            toSlot.Item = fromSlot.Item;
            toSlot.Amount = amount;

            fromSlot.Amount -= amount;

            if (fromSlot.Amount <= 0)
            {
                fromSlot.Clear();
            }

            NotifyChanged();
            return true;
        }

        if (toSlot.Item != fromSlot.Item)
        {
            return false;
        }

        if (!toSlot.Item.Stackable)
        {
            return false;
        }

        int freeSpace = toSlot.Item.MaxStack - toSlot.Amount;
        if (freeSpace <= 0)
        {
            return false;
        }

        int moveAmount = Mathf.Min(amount, freeSpace);
        toSlot.Amount += moveAmount;
        fromSlot.Amount -= moveAmount;

        if (fromSlot.Amount <= 0)
        {
            fromSlot.Clear();
        }

        NotifyChanged();
        return true;
    }

    public int CountItem(ItemData item)
    {
        if (item == null)
        {
            return 0;
        }

        int total = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (slot.IsEmpty)
            {
                continue;
            }

            if (slot.Item == item)
            {
                total += slot.Amount;
            }
        }

        return total;
    }

    public bool HasItem(ItemData item, int amount)
    {
        return CountItem(item) >= amount;
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < slots.Count;
    }

    private void NotifyChanged()
    {
        OnChanged?.Invoke();
    }

    public void ForceNotifyChanged()
    {
        OnChanged?.Invoke();
    }
}
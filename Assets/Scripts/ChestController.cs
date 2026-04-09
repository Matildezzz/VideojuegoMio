using System;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount = 12;
    public GameObject[] itemPrefabs;

    private Dictionary<int, int> itemsCountCache = new Dictionary<int, int>();
    public event Action OnChestChanged;

    private void Start()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();

        BuildEmptySlots();

        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            if (itemPrefabs[i] == null)
            {
                continue;
            }

            Item item = itemPrefabs[i].GetComponent<Item>();
            int amount = item != null ? item.quantity : 1;

            AddItem(itemPrefabs[i], amount);
        }

        RebuildItemCounts();
    }

    private void BuildEmptySlots()
    {
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>();
            ConfigureSlot(slot, i);
        }
    }

    private void ConfigureSlot(Slot slot, int index)
    {
        slot.owner = this;
        slot.slotIndex = index;
        slot.currentItem = null;
    }

    public Slot GetSlot(int index)
    {
        if (index < 0 || index >= inventoryPanel.transform.childCount)
        {
            return null;
        }

        return inventoryPanel.transform.GetChild(index).GetComponent<Slot>();
    }

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();

                if (item != null)
                {
                    if (!itemsCountCache.ContainsKey(item.ID))
                    {
                        itemsCountCache[item.ID] = 0;
                    }

                    itemsCountCache[item.ID] += item.quantity;
                }
            }
        }

        OnChestChanged?.Invoke();
    }

    public bool AddItem(GameObject itemPrefab, int amount)
    {
        if (itemPrefab == null)
        {
            return false;
        }

        if (amount <= 0)
        {
            return false;
        }

        Item itemToAdd = itemPrefab.GetComponent<Item>();

        if (itemToAdd == null)
        {
            return false;
        }

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();

                if (slotItem != null && slotItem.ID == itemToAdd.ID)
                {
                    slotItem.AddToStack(amount);
                    RebuildItemCounts();
                    return true;
                }
            }
        }

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                Item newItemComponent = newItem.GetComponent<Item>();

                if (newItemComponent != null)
                {
                    newItemComponent.quantity = amount;
                    newItemComponent.UpdateQuantityDisplay();
                }

                slot.currentItem = newItem;
                RebuildItemCounts();
                return true;
            }
        }

        Debug.Log("Chest is full");
        return false;
    }

    public bool AddItemByID(int itemID, int amount)
    {
        if (itemDictionary == null)
        {
            itemDictionary = FindFirstObjectByType<ItemDictionary>();
        }

        if (itemDictionary == null)
        {
            return false;
        }

        GameObject itemPrefab = itemDictionary.GetItemPrefab(itemID);

        if (itemPrefab == null)
        {
            return false;
        }

        return AddItem(itemPrefab, amount);
    }

    public bool RemoveFromSlot(int slotIndex, int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        Slot slot = GetSlot(slotIndex);

        if (slot == null || slot.currentItem == null)
        {
            return false;
        }

        Item item = slot.currentItem.GetComponent<Item>();

        if (item == null)
        {
            return false;
        }

        int removed = item.RemoveFromStack(amount);

        if (item.quantity <= 0)
        {
            Destroy(slot.currentItem);
            slot.currentItem = null;
        }

        RebuildItemCounts();
        return removed > 0;
    }
}
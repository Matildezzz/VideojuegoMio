using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    public static InventoryController Instance { get; private set; }

    private Dictionary<int, int> itemsCountCache = new Dictionary<int, int>();
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();

        for (int i = 0; i < slotCount; i++)
        {
            Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>();
            ConfigureSlot(slot, i);

            if (i < itemPrefabs.Length && itemPrefabs[i] != null)
            {
                GameObject item = CreateUIItemInSlot(itemPrefabs[i], slot.transform, 1);
                slot.currentItem = item;
            }
        }

        RebuildItemCounts();
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

        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts()
    {
        return itemsCountCache;
    }

    public bool AddItem(GameObject itemPrefab)
    {
        return AddItem(itemPrefab, 1);
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
                GameObject newItem = CreateUIItemInSlot(itemPrefab, slotTransform, amount);
                slot.currentItem = newItem;
                RebuildItemCounts();
                return true;
            }
        }

        Debug.Log("Inventory is full");
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

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();

                if (item != null)
                {
                    invData.Add(new InventorySaveData
                    {
                        itemID = item.ID,
                        slotIndex = slotTransform.GetSiblingIndex(),
                        quantity = item.quantity
                    });
                }
            }
        }

        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
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

        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < 0 || data.slotIndex >= inventoryPanel.transform.childCount)
            {
                continue;
            }

            Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);

            if (itemPrefab != null)
            {
                GameObject item = CreateUIItemInSlot(itemPrefab, slot.transform, data.quantity);
                slot.currentItem = item;
            }
        }

        RebuildItemCounts();
    }

    public void RemoveItemsFromInventory(int itemID, int amountToRemove)
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            if (amountToRemove <= 0)
            {
                break;
            }

            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();

                if (item != null && item.ID == itemID)
                {
                    int removed = Mathf.Min(amountToRemove, item.quantity);
                    item.RemoveFromStack(removed);
                    amountToRemove -= removed;

                    if (item.quantity == 0)
                    {
                        Destroy(slot.currentItem);
                        slot.currentItem = null;
                    }
                }
            }
        }

        RebuildItemCounts();
    }

    private GameObject CreateUIItemInSlot(GameObject itemPrefab, Transform slotTransform, int amount)
{
    GameObject newItem = Instantiate(itemPrefab, slotTransform);

    RectTransform rt = newItem.GetComponent<RectTransform>();
    if (rt != null)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.sizeDelta = new Vector2(80f, 80f);
    }

    Image image = newItem.GetComponent<Image>();
    if (image != null)
    {
        image.preserveAspect = true;
    }

    Item itemComponent = newItem.GetComponent<Item>();
    if (itemComponent != null)
    {
        itemComponent.quantity = amount;
        itemComponent.UpdateQuantityDisplay();
    }

    newItem.transform.SetAsLastSibling();
    return newItem;
}
}
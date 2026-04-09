using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 10; // 1-0 on the keyboard

    private ItemDictionary itemDictionary;
    private Key[] hotbarKeys;

    private void Awake()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();

        hotbarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }
    }

    private void Start()
    {
        if (hotbarPanel == null)
        {
            return;
        }

        if (hotbarPanel.transform.childCount == 0)
        {
            for (int i = 0; i < slotCount; i++)
            {
                Instantiate(slotPrefab, hotbarPanel.transform);
            }
        }

        for (int i = 0; i < hotbarPanel.transform.childCount; i++)
        {
            Slot slot = hotbarPanel.transform.GetChild(i).GetComponent<Slot>();

            if (slot != null)
            {
                slot.owner = this;
                slot.slotIndex = i;
                slot.currentItem = null;
            }
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                UseItemInSlot(i);
            }
        }
    }

    private void UseItemInSlot(int index)
    {
        Slot slot = GetSlot(index);

        if (slot == null || slot.currentItem == null)
        {
            return;
        }

        Item item = slot.currentItem.GetComponent<Item>();
        if (item != null)
        {
            item.UseItem();
            return;
        }

        InventoryItemUI uiItem = slot.currentItem.GetComponent<InventoryItemUI>();
        if (uiItem != null && uiItem.itemData != null)
        {
            Debug.Log("Item en hotbar: " + uiItem.itemData.itemName);
        }
    }

    public Slot GetSlot(int index)
    {
        if (hotbarPanel == null)
        {
            return null;
        }

        if (index < 0 || index >= hotbarPanel.transform.childCount)
        {
            return null;
        }

        return hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
    }

    public bool AddItemByData(ItemData itemData, int amount = 1)
    {
        if (itemData == null || amount <= 0)
        {
            return false;
        }

        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null || slot.currentItem == null)
            {
                continue;
            }

            InventoryItemUI slotItem = slot.currentItem.GetComponent<InventoryItemUI>();

            if (slotItem != null && slotItem.itemData != null && slotItem.itemData.ID == itemData.ID)
            {
                slotItem.AddToStack(amount);
                return true;
            }

            Item classicItem = slot.currentItem.GetComponent<Item>();

            if (classicItem != null && classicItem.ID == itemData.ID)
            {
                classicItem.AddToStack(amount);
                return true;
            }
        }

        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null || slot.currentItem != null)
            {
                continue;
            }

            GameObject newItem = CreateUIItemInSlot(itemData, slotTransform, amount);

            if (newItem == null)
            {
                return false;
            }

            slot.currentItem = newItem;
            return true;
        }

        return false;
    }

    private GameObject CreateUIItemInSlot(ItemData itemData, Transform slotTransform, int amount)
    {
        if (itemData == null)
        {
            return null;
        }

        if (itemData.inventoryPrefab == null)
        {
            Debug.LogWarning("El item " + itemData.itemName + " no tiene inventoryPrefab asignado.");
            return null;
        }

        GameObject newItem = Instantiate(itemData.inventoryPrefab, slotTransform);

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

        if (newItem.GetComponent<CanvasGroup>() == null)
        {
            newItem.AddComponent<CanvasGroup>();
        }

        if (newItem.GetComponent<ItemDragHandler>() == null)
        {
            newItem.AddComponent<ItemDragHandler>();
        }

        InventoryItemUI itemUI = newItem.GetComponent<InventoryItemUI>();
        if (itemUI == null)
        {
            itemUI = newItem.AddComponent<InventoryItemUI>();
        }

        itemUI.itemData = itemData;
        itemUI.quantity = amount;
        itemUI.RefreshUI();

        newItem.transform.SetAsLastSibling();
        return newItem;
    }

    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();

        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null || slot.currentItem == null)
            {
                continue;
            }

            InventoryItemUI uiItem = slot.currentItem.GetComponent<InventoryItemUI>();
            if (uiItem != null && uiItem.itemData != null)
            {
                invData.Add(new InventorySaveData
                {
                    itemID = uiItem.itemData.ID,
                    slotIndex = slotTransform.GetSiblingIndex(),
                    quantity = uiItem.quantity
                });

                continue;
            }

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

        return invData;
    }

    public void SetHotbarItems(List<InventorySaveData> inventorySaveData)
    {
        foreach (Transform child in hotbarPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Slot slot = Instantiate(slotPrefab, hotbarPanel.transform).GetComponent<Slot>();
            slot.owner = this;
            slot.slotIndex = i;
            slot.currentItem = null;
        }

        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < 0 || data.slotIndex >= hotbarPanel.transform.childCount)
            {
                continue;
            }

            Slot slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);

            if (itemPrefab != null)
            {
                GameObject item = Instantiate(itemPrefab, slot.transform);

                RectTransform rt = item.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one;
                }

                Item classicItem = item.GetComponent<Item>();
                if (classicItem != null)
                {
                    classicItem.quantity = data.quantity;
                    classicItem.UpdateQuantityDisplay();
                }

                InventoryItemUI uiItem = item.GetComponent<InventoryItemUI>();
                if (uiItem != null)
                {
                    uiItem.quantity = data.quantity;
                    uiItem.RefreshUI();
                }

                slot.currentItem = item;
            }
        }
    }
}
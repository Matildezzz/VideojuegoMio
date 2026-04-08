using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour 
{ 
    private ItemDictionary itemDictionary; 
    public GameObject inventoryPanel; 
    public GameObject slotPrefab; 
    public int slotCount; 
    public GameObject[] itemPrefabs; 

    public static InventoryController Instance {get; private set;} 
    Dictionary<int, int> itemsCountCache = new(); 
    public event Action OnInventoryChanged; // Event to notify quest system (or any other that needs to know) 
    
    private void Awake() { 
        if(Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        } 
        Instance = this; 
    } 
    
    void Start() 
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>(); 
        RebuildItemCounts(); 
        
        for(int i = 0; i < slotCount; i++) 
        {
            Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>(); 
            
            if(i < itemPrefabs.Length) 
            {
                GameObject item = Instantiate(itemPrefabs[i], slot.transform); 
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; 
                slot.currentItem = item; 
            }    
        } 
    }

    public void RebuildItemCounts() 
    {
        itemsCountCache.Clear(); 
        foreach(Transform slotTransform in inventoryPanel.transform) 
        {
            Slot slot = slotTransform.GetComponent<Slot>(); 
            if(slot.currentItem != null) 
            {
                Item item = slot.currentItem.GetComponent<Item>(); 
                if(item != null) 
                {
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + item.quantity; 
                }
            }
        }            
        OnInventoryChanged?.Invoke();
    }
                          
    public Dictionary<int,int> GetItemCounts() => itemsCountCache; 
    
    public bool AddItem(GameObject itemPrefab) 
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>(); 
        if(itemToAdd == null) return false; // Check if we have this item in inventory 
        foreach(Transform slotTransform in inventoryPanel.transform) 
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null) 
            { 
                Item slotItem = slot.currentItem.GetComponent<Item>(); 
                if (slotItem != null && slotItem.ID == itemToAdd.ID) 
                {
                    // Same item, stack them 
                    slotItem.AddToStack(); 
                    RebuildItemCounts(); 
                    return true; 
                } 
            } 
        } 

        // Look for empty slot 
        foreach(Transform slotTransform in inventoryPanel.transform) 
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null) 
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform); 
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; 
                slot.currentItem = newItem; 
                RebuildItemCounts(); 
                return true; 
            } 
        } 
        Debug.Log("Inventory is full"); 
        return false; 
    } 
                            
    public List<InventorySaveData> GetInventoryItems() 
    { 
        List<InventorySaveData> invData = new List<InventorySaveData>(); 
        foreach (Transform slotTransform in inventoryPanel.transform) 
        { 
            Slot slot = slotTransform.GetComponent<Slot>(); 
            if(slot.currentItem != null) 
            { 
                Item item = slot.currentItem.GetComponent<Item>(); 
                invData.Add(new InventorySaveData{ 
                    itemID = item.ID, 
                    slotIndex = slotTransform.GetSiblingIndex(), 
                    quantity = item.quantity 
                }); 
            } 
        } 
        return invData; 
        } 
                    
    public void SetInventoryItems(List<InventorySaveData> inventorySaveData) 
    { 
        // Clear inventory panel - avoid duplicates 
        foreach(Transform child in inventoryPanel.transform) 
        { 
            Destroy(child.gameObject); 
        } 
        
        // Create new slots 
        for(int i = 0; i < slotCount; i++) 
        { 
            Instantiate(slotPrefab, inventoryPanel.transform); 
        } 
        
        // Populate slots with saved items 
        foreach(InventorySaveData data in inventorySaveData) 
        { 
            Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>(); 
            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID); 
            
            if(itemPrefab != null) 
            { 
                GameObject item = Instantiate(itemPrefab, slot.transform); 
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; 
                Item itemComponent = item.GetComponent<Item>(); 
                
                if(itemComponent != null) 
                { 
                    itemComponent.quantity = data.quantity; 
                    itemComponent.UpdateQuantityDisplay(); 
                } slot.currentItem = item; 
            } 
        } 
        RebuildItemCounts(); 
    } 
    
    public void RemoveItemsFromInventory(int itemID, int amountToRemove) 
    { 
        foreach(Transform slotTransform in inventoryPanel.transform) 
        { 
            if(amountToRemove <= 0) break; 
            Slot slot = slotTransform.GetComponent<Slot>(); 
            
            if(slot?.currentItem?.GetComponent<Item>() is Item item && item.ID == itemID) 
            { 
                int removed = Mathf.Min(amountToRemove, item.quantity); 
                item.RemoveFromStack(removed); 
                amountToRemove -= removed; 
                
                if(item.quantity == 0) 
                { 
                    Destroy(slot.currentItem); 
                    slot.currentItem = null; 
                } 
            } 
        } 
        RebuildItemCounts(); 
    } 
}
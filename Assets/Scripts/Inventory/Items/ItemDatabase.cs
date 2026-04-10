using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Gameplay/Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    private Dictionary<string, ItemData> itemsById;

    public IReadOnlyList<ItemData> Items => items;

    private void OnEnable()
    {
        BuildDictionary();
    }

    public void BuildDictionary()
    {
        itemsById = new Dictionary<string, ItemData>();

        for (int i = 0; i < items.Count; i++)
        {
            ItemData item = items[i];

            if (item == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.ItemId))
            {
                Debug.LogWarning("ItemDatabase: hay un item sin ItemId -> " + item.name);
                continue;
            }

            if (itemsById.ContainsKey(item.ItemId))
            {
                Debug.LogWarning("ItemDatabase: ItemId duplicado -> " + item.ItemId);
                continue;
            }

            itemsById.Add(item.ItemId, item);
        }
    }

    public ItemData GetItemById(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        if (itemsById == null)
        {
            BuildDictionary();
        }

        ItemData itemData;
        if (itemsById.TryGetValue(itemId, out itemData))
        {
            return itemData;
        }

        Debug.LogWarning("ItemDatabase: no existe item con id -> " + itemId);
        return null;
    }
}
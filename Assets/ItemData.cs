using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    public int ID;
    public string itemName;
    public Sprite inventoryIcon;
    public GameObject worldPrefab;
    public GameObject inventoryPrefab;
    public int buyPrice = 10;

    [Range(0f, 1f)]
    public float sellPriceMultiplier = 0.5f;
}
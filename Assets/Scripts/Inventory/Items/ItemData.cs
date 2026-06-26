using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [TextArea(2, 4)] [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject worldPrefab;

    [Header("Percepcion alienigena")]
    [SerializeField] private string alienDisplayName;
    [TextArea(2, 4)] [SerializeField] private string alienDescription;

    [Header("Stack")]
    [SerializeField] private bool stackable = true;
    [SerializeField] private int maxStack = 25;

    [Header("Category")]
    [SerializeField] private ItemCategory category = ItemCategory.Resource;

    [Header("Economy")]
    [SerializeField] private int buyPrice = 10;
    [SerializeField, Range(0f, 1f)] private float sellPriceMultiplier = 0.5f;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public string Description => description;
    public string AlienDisplayName => alienDisplayName;
    public string AlienDescription => alienDescription;
    public Sprite Icon => icon;
    public GameObject WorldPrefab => worldPrefab;
    public bool Stackable => stackable;
    public int MaxStack => maxStack;
    public ItemCategory Category => category;
    public int BuyPrice => buyPrice;

    public int GetSellPrice()
    {
        return Mathf.RoundToInt(buyPrice * sellPriceMultiplier);
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (maxStack < 1)
        {
            maxStack = 1;
        }

        if (!stackable)
        {
            maxStack = 1;
        }

        if (buyPrice < 0)
        {
            buyPrice = 0;
        }

        sellPriceMultiplier = Mathf.Clamp01(sellPriceMultiplier);
    }
#endif
}

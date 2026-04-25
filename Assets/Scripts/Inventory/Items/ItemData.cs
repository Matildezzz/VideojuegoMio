using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [Header("Identificacion")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;

    [Header("Descripcion")]
    [SerializeField, TextArea(2, 5)] private string description;

    [Header("Visual")]
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject worldPrefab;

    [Header("Stack")]
    [SerializeField] private bool stackable = true;
    [SerializeField] private int maxStack = 99;

    [Header("Categoria")]
    [SerializeField] private ItemCategory category = ItemCategory.None;

    [Header("Economia")]
    [SerializeField] private int buyPrice = 10;
    [SerializeField, Range(0f, 1f)] private float sellPriceMultiplier = 0.5f;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public string Description => description;
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

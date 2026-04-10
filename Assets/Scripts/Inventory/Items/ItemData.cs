using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [Header("Identificacion")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;

    [Header("Visual")]
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject worldPrefab;

    [Header("Stack")]
    [SerializeField] private bool stackable = true;
    [SerializeField] private int maxStack = 99;

    [Header("Categoria")]
    [SerializeField] private ItemCategory category = ItemCategory.None;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject WorldPrefab => worldPrefab;
    public bool Stackable => stackable;
    public int MaxStack => maxStack;
    public ItemCategory Category => category;

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
    }
#endif
}
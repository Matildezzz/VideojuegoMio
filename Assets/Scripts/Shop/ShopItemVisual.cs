using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemVisual : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;

    private ItemData itemData;
    private int quantity = 1;

    private void Awake()
    {
        FindReferencesIfNeeded();
        RefreshUI();
    }

    public void Configure(ItemData newItemData, int newQuantity)
    {
        itemData = newItemData;
        quantity = newQuantity;
        FindReferencesIfNeeded();
        RefreshUI();
    }

    private void FindReferencesIfNeeded()
    {
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }

        if (iconImage == null)
        {
            iconImage = GetComponentInChildren<Image>(true);
        }

        if (quantityText == null)
        {
            quantityText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void RefreshUI()
    {
        bool hasItem = itemData != null;

        if (iconImage != null)
        {
            iconImage.enabled = hasItem;
            iconImage.sprite = hasItem ? itemData.Icon : null;
            iconImage.preserveAspect = true;
        }

        if (quantityText != null)
        {
            quantityText.text = hasItem ? quantity.ToString() : string.Empty;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    public ItemData itemData;
    public int quantity = 1;

    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;

    private void Awake()
    {
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (itemData != null && iconImage != null)
        {
            //iconImage.sprite = itemData.inventoryIcon;
            iconImage.preserveAspect = true;
        }

        if (quantityText != null)
        {
            quantityText.text = quantity.ToString();
        }
    }

    public void AddToStack(int amount = 1)
    {
        quantity += amount;
        RefreshUI();
    }

    public int RemoveFromStack(int amount = 1)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        RefreshUI();
        return removed;
    }
}
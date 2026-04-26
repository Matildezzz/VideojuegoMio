using TMPro;
using UnityEngine;

public class ShopSlot : MonoBehaviour
{
    public GameObject currentItem;
    public int itemPrice;
    public TMP_Text priceText;
    public bool isShopSlot = true;

    private void Awake()
    {
        if (priceText == null)
        {
            Transform priceTransform = transform.Find("PriceText");
            if (priceTransform != null)
            {
                priceText = priceTransform.GetComponent<TMP_Text>();
            }
        }

        UpdatePriceDisplay();
    }

    public void SetItem(GameObject item, int price)
    {
        SetItem(item, price, isShopSlot);
    }

    public void SetItem(GameObject item, int price, bool showAsBuyPrice)
    {
        currentItem = item;
        itemPrice = price;
        isShopSlot = showAsBuyPrice;
        UpdatePriceDisplay();
    }

    public void UpdatePriceDisplay()
    {
        if (priceText == null)
        {
            return;
        }

        if (currentItem == null)
        {
            priceText.text = string.Empty;
            return;
        }

        priceText.text = isShopSlot ? itemPrice + " oro" : "Venta: " + itemPrice + " oro";
    }
}

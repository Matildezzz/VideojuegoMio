using TMPro;
using UnityEngine;

public class ShopSlot : MonoBehaviour
{
    public GameObject currentItem;
    public int itemPrice;
    public TMP_Text priceText;
    public bool isShopSlot = true; // In shop menu, true = shop side and false = player side

    void Awake()
    {
        if (!priceText)
        {
            priceText = transform.Find("PriceText").GetComponent<TMP_Text>();
        }
    }

    public void UpdatePriceDisplay()
    {
        if(priceText && currentItem)
        {
            priceText.text = itemPrice.ToString();
        }
    }

    public void SetItem(GameObject item, int price)
    {
        currentItem = item;
        itemPrice = price;
        UpdatePriceDisplay();
    }
}

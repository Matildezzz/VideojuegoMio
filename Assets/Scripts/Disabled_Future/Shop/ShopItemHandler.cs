using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ShopItemVisual))]
public class ShopItemHandler : MonoBehaviour, IPointerClickHandler
{
    private bool isShopItem;
    private ItemData itemData;
    private InventoryUISlotSource originalSource = InventoryUISlotSource.Backpack;
    private int originalSlotIndex = -1;
    private int availableAmount = 1;

    public void Initialise(
        bool shopItem,
        ItemData data,
        InventoryUISlotSource source = InventoryUISlotSource.Backpack,
        int slotIndex = -1,
        int amount = 1)
    {
        isShopItem = shopItem;
        itemData = data;
        originalSource = source;
        originalSlotIndex = slotIndex;
        availableAmount = Mathf.Max(1, amount);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ShopController.Instance == null || itemData == null)
        {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            SelectItem();
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        if (isShopItem)
        {
            ShopController.Instance.TryBuyItem(itemData, 1);
        }
        else
        {
            ShopController.Instance.TrySellItem(itemData, originalSource, originalSlotIndex, 1);
        }
    }

    private void SelectItem()
    {
        if (isShopItem)
        {
            ShopController.Instance.SelectShopItem(itemData, availableAmount);
        }
        else
        {
            ShopController.Instance.SelectPlayerItem(itemData, originalSource, originalSlotIndex, availableAmount);
        }
    }
}

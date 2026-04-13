using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InventoryItemUI))]
public class ShopItemHandler : MonoBehaviour, IPointerClickHandler
{
    private bool isShopItem;
    private ItemData itemData;
    private InventoryUISlotSource originalSource = InventoryUISlotSource.Backpack;
    private int originalSlotIndex = -1;

    public void Initialise(
        bool shopItem,
        ItemData data,
        InventoryUISlotSource source = InventoryUISlotSource.Backpack,
        int slotIndex = -1)
    {
        isShopItem = shopItem;
        itemData = data;
        originalSource = source;
        originalSlotIndex = slotIndex;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        if (ShopController.Instance == null || itemData == null)
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
}
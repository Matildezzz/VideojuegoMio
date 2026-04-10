using UnityEngine;

public sealed class ChestTransferController : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;

    private ChestInventory currentChest;

    public void SetCurrentChest(ChestInventory chest)
    {
        currentChest = chest;
    }

    public void ClearCurrentChest()
    {
        currentChest = null;
    }

    public bool MoveFromBackpackToChest(int backpackIndex, int amount = 0)
    {
        if (playerInventory == null || currentChest == null)
        {
            return false;
        }

        return playerInventory.Backpack.MoveSlotTo(currentChest.Container, backpackIndex, amount);
    }

    public bool MoveFromHotbarToChest(int hotbarIndex, int amount = 0)
    {
        if (playerInventory == null || currentChest == null)
        {
            return false;
        }

        return playerInventory.Hotbar.MoveSlotTo(currentChest.Container, hotbarIndex, amount);
    }

    public bool MoveFromChestToBackpack(int chestIndex, int amount = 0)
    {
        if (playerInventory == null || currentChest == null)
        {
            return false;
        }

        int left = 0;

        InventorySlot slot = currentChest.Container.GetSlot(chestIndex);
        if (slot == null || slot.IsEmpty)
        {
            return false;
        }

        int amountToMove = amount <= 0 ? slot.Amount : Mathf.Min(amount, slot.Amount);

        left = playerInventory.Hotbar.AddItem(slot.Item, amountToMove);
        left = playerInventory.Backpack.AddItem(slot.Item, left);

        int moved = amountToMove - left;
        if (moved <= 0)
        {
            return false;
        }

        currentChest.Container.RemoveFromSlot(chestIndex, moved);
        return true;
    }
}
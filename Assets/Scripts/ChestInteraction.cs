using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ChestInteraction : MonoBehaviour
{
    [SerializeField] private ChestController chestController;
    [SerializeField] private GameObject chestWindow;
    [SerializeField] private string playerTag = "Player";

    private bool playerInRange = false;

    public static ChestInteraction CurrentOpenChest { get; private set; }

    private void Awake()
    {
        if (chestController == null)
        {
            chestController = GetComponent<ChestController>();
        }

        if (chestWindow != null)
        {
            chestWindow.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (CurrentOpenChest == this)
            {
                CloseChest();
            }
            else
            {
                OpenChest();
            }
        }

        if (CurrentOpenChest == this && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseChest();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;

            if (CurrentOpenChest == this)
            {
                CloseChest();
            }
        }
    }

    private void OpenChest()
    {
        if (CurrentOpenChest != null && CurrentOpenChest != this)
        {
            CurrentOpenChest.CloseChest();
        }

        CurrentOpenChest = this;

        if (chestWindow != null)
        {
            chestWindow.SetActive(true);
        }
    }

    private void CloseChest()
    {
        if (CurrentOpenChest == this)
        {
            CurrentOpenChest = null;
        }

        if (chestWindow != null)
        {
            chestWindow.SetActive(false);
        }
    }

    public void HandleSlotClick(Slot clickedSlot, PointerEventData.InputButton button)
    {
        if (CurrentOpenChest != this)
        {
            return;
        }

        if (clickedSlot == null || clickedSlot.currentItem == null)
        {
            return;
        }

        if (InventoryController.Instance == null)
        {
            return;
        }

        Item clickedItem = clickedSlot.currentItem.GetComponent<Item>();

        if (clickedItem == null)
        {
            return;
        }

        int amountToMove = button == PointerEventData.InputButton.Right ? 1 : clickedItem.quantity;

        if (clickedSlot.owner == InventoryController.Instance)
        {
            MoveFromPlayerToChest(clickedSlot.slotIndex, amountToMove);
        }
        else if (clickedSlot.owner == chestController)
        {
            MoveFromChestToPlayer(clickedSlot.slotIndex, amountToMove);
        }
    }

    private void MoveFromPlayerToChest(int slotIndex, int amount)
    {
        Slot playerSlot = InventoryController.Instance.GetSlot(slotIndex);

        if (playerSlot == null || playerSlot.currentItem == null)
        {
            return;
        }

        Item item = playerSlot.currentItem.GetComponent<Item>();

        if (item == null)
        {
            return;
        }

        amount = Mathf.Clamp(amount, 1, item.quantity);

        bool added = chestController.AddItemByID(item.ID, amount);

        if (!added)
        {
            return;
        }

        InventoryController.Instance.RemoveFromSlot(slotIndex, amount);
    }

    private void MoveFromChestToPlayer(int slotIndex, int amount)
    {
        Slot chestSlot = chestController.GetSlot(slotIndex);

        if (chestSlot == null || chestSlot.currentItem == null)
        {
            return;
        }

        Item item = chestSlot.currentItem.GetComponent<Item>();

        if (item == null)
        {
            return;
        }

        amount = Mathf.Clamp(amount, 1, item.quantity);

        bool added = InventoryController.Instance.AddItemByID(item.ID, amount);

        if (!added)
        {
            return;
        }

        chestController.RemoveFromSlot(slotIndex, amount);
    }
}
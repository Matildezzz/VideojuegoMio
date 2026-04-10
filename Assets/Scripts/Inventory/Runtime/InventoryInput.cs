using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InventoryInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InventoryUIController inventoryUIController;

    [Header("Hotbar")]
    [SerializeField] private bool wrapHotbarSelection = true;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }
    }

    private void Update()
    {
        if (playerInventory == null)
        {
            return;
        }

        HandleHotbarNumberKeys();
        HandleMouseWheel();
        HandleUseSelectedItem();
        HandleDropSelectedItem();
        HandleToggleInventory();

        HandleGamepadInput();
    }

    private void HandleHotbarNumberKeys()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame) playerInventory.SelectSlot(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) playerInventory.SelectSlot(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) playerInventory.SelectSlot(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) playerInventory.SelectSlot(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) playerInventory.SelectSlot(4);
    }

    private void HandleMouseWheel()
    {
        if (Mouse.current == null)
        {
            return;
        }

        float wheel = Mouse.current.scroll.ReadValue().y;

        if (wheel > 0f)
        {
            ChangeSelectedHotbarSlot(-1);
        }
        else if (wheel < 0f)
        {
            ChangeSelectedHotbarSlot(1);
        }
    }

    private void HandleUseSelectedItem()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            playerInventory.UseSelectedItem();
        }
    }

    private void HandleDropSelectedItem()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            playerInventory.DropSelectedItem();
        }
    }

    private void HandleToggleInventory()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (inventoryUIController != null)
            {
                inventoryUIController.ToggleInventory();
            }
        }
    }

    private void ChangeSelectedHotbarSlot(int delta)
    {
        int slotCount = playerInventory.Hotbar.SlotCount;
        if (slotCount <= 0)
        {
            return;
        }

        int newIndex = playerInventory.SelectedHotbarIndex + delta;

        if (wrapHotbarSelection)
        {
            if (newIndex < 0)
            {
                newIndex = slotCount - 1;
            }
            else if (newIndex >= slotCount)
            {
                newIndex = 0;
            }
        }
        else
        {
            newIndex = Mathf.Clamp(newIndex, 0, slotCount - 1);
        }

        playerInventory.SelectSlot(newIndex);
    }

    private void HandleGamepadInput()
    {
        if (Gamepad.current == null)
        {
            return;
        }

        if (Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            ChangeSelectedHotbarSlot(-1);
        }

        if (Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            ChangeSelectedHotbarSlot(1);
        }

        if (Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            playerInventory.UseSelectedItem();
        }

        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            playerInventory.DropSelectedItem();
        }

        if (Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            if (inventoryUIController != null)
            {
                inventoryUIController.ToggleInventory();
            }
        }
    }
}
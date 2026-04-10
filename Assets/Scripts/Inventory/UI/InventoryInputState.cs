using UnityEngine.InputSystem;

public static class InventoryInputState
{
    public static bool IsQuickMoveModifierPressed()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
    }
}
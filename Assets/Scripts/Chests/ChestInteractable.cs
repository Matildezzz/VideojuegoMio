using UnityEngine;

public sealed class ChestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private ChestInventory chestInventory;
    [SerializeField] private ChestUIController chestUIController;

    public string InteractionText
    {
        get
        {
            if (chestUIController != null && chestInventory != null && chestUIController.IsOpen && chestUIController.CurrentChest == chestInventory)
            {
                return "E - Cerrar cofre";
            }

            return "E - Abrir cofre";
        }
    }

    private void Awake()
    {
        if (chestInventory == null)
        {
            chestInventory = GetComponent<ChestInventory>();
        }

        if (chestUIController == null)
        {
            chestUIController = FindFirstObjectByType<ChestUIController>();
        }
    }

    public bool CanInteract()
    {
        return chestInventory != null && chestUIController != null;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        if (chestUIController.IsOpen && chestUIController.CurrentChest == chestInventory)
        {
            chestUIController.CloseChest();
        }
        else
        {
            chestUIController.OpenChest(chestInventory);
        }
    }
}

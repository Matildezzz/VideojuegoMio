using UnityEngine;

public class MuseumDonationInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerInventory playerInventory;

    public string InteractionText => "E - Donar al museo";

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (playerInventory == null)
        {
            ShowError("No se encontró el inventario del jugador.");
            return;
        }

        if (MuseumController.Instance == null)
        {
            ShowError("No se encontró el museo en la escena.");
            return;
        }

        ItemData selectedItem = playerInventory.GetSelectedItem();

        if (selectedItem == null)
        {
            ShowWarning("Selecciona un objeto para donar.");
            return;
        }

        if (!(selectedItem is MuseumItemData))
        {
            ShowWarning("Este objeto no se puede donar al museo.");
            return;
        }

        if (MuseumController.Instance.IsDonated(selectedItem))
        {
            ShowWarning("Este objeto ya fue donado.");
            return;
        }

        bool donated = MuseumController.Instance.DonateItem(playerInventory, selectedItem);

        if (donated)
        {
            ShowSuccess("Has donado " + selectedItem.DisplayName + " al museo.");
        }
        else
        {
            ShowError("No se pudo donar este objeto.");
        }
    }

    private void ShowSuccess(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Success, "Coin");
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void ShowWarning(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Warning, "Error");
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void ShowError(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Error, "Error");
        }
        else
        {
            Debug.Log(message);
        }
    }
}

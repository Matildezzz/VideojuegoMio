using UnityEngine;

public class MuseumDonationInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerInventory playerInventory;

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

        if (playerInventory == null || MuseumController.Instance == null)
        {
            return;
        }

        ItemData selectedItem = playerInventory.GetSelectedItem();

        if (selectedItem == null)
        {
            Debug.Log("Museo: no llevas ningún objeto seleccionado.");
            return;
        }

        if (!(selectedItem is MuseumItemData))
        {
            Debug.Log("Museo: este objeto no se puede donar.");
            return;
        }

        if (MuseumController.Instance.IsDonated(selectedItem))
        {
            Debug.Log("Museo: este objeto ya fue donado.");
            return;
        }

        bool donated = MuseumController.Instance.DonateItem(playerInventory, selectedItem);

        if (donated)
        {
            Debug.Log("Museo: objeto donado correctamente.");
        }
    }
}
using UnityEngine;

public sealed class ItemPickup : MonoBehaviour
{
    [Header("Pickup Data")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    public ItemData ItemData => itemData;
    public int Amount => amount;

    public bool TryPickup(IItemReceiver receiver)
    {
        if (receiver == null)
        {
            return false;
        }

        if (itemData == null || amount <= 0)
        {
            Debug.LogWarning("ItemPickup: itemData es null o amount <= 0 en " + gameObject.name);
            return false;
        }

        bool pickedUp = receiver.TryAddItem(itemData, amount);

        if (pickedUp)
        {
            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("+" + amount + " " + itemData.DisplayName, ToastType.Success, "Pickup");
            }

            Destroy(gameObject);
        }
        else
        {
            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("No hay espacio en el inventario.", ToastType.Error, "Error");
            }
        }

        return pickedUp;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (amount < 1)
        {
            amount = 1;
        }
    }
#endif
}
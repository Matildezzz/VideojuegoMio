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
            Destroy(gameObject);
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
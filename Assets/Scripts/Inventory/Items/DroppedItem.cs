using UnityEngine;

public sealed class DroppedItem : MonoBehaviour
{
    [Header("Runtime Data")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer iconRenderer;

    private float pickupBlockedUntil;

    public ItemData ItemData => itemData;
    public int Amount => amount;

    public void Setup(ItemData item, int newAmount)
    {
        itemData = item;
        amount = Mathf.Max(1, newAmount);

        RefreshVisual();
    }

    public void BlockPickupFor(float seconds)
    {
        pickupBlockedUntil = Time.time + Mathf.Max(0f, seconds);
    }

    public bool CanBePickedUp()
    {
        return Time.time >= pickupBlockedUntil;
    }

    public bool TryPickup(IItemReceiver receiver)
    {
        if (!CanBePickedUp())
        {
            return false;
        }

        if (receiver == null)
        {
            return false;
        }

        if (itemData == null || amount <= 0)
        {
            Debug.LogWarning("DroppedItem: itemData es null o amount <= 0 en " + gameObject.name);
            return false;
        }

        bool pickedUp = receiver.TryAddItem(itemData, amount);

        if (pickedUp)
        {
            Destroy(gameObject);
        }

        return pickedUp;
    }

    private void Awake()
    {
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (iconRenderer != null && itemData != null)
        {
            iconRenderer.sprite = itemData.Icon;
        }
    }
}
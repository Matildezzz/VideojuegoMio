using System;
using UnityEngine;

public sealed class PlayerInventory : MonoBehaviour, IItemReceiver
{
    public event Action OnInventoryChanged;
    public event Action<int> OnSelectedHotbarChanged;

    [Header("Containers")]
    [SerializeField] private int hotbarSize = 5;
    [SerializeField] private int backpackSize = 20;
    [SerializeField] private InventoryContainer hotbar = new InventoryContainer();
    [SerializeField] private InventoryContainer backpack = new InventoryContainer();

    [Header("Selection")]
    [SerializeField] private int selectedHotbarIndex = 0;

    [Header("Drop")]
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float dropDistance = 1.25f;
    [SerializeField] private ItemDropSpawner itemDropSpawner;

    //private PlayerHealth playerHealth;
    //private PlayerEnergy playerEnergy;

    [SerializeField] private PlayerItemActions itemUseService;

    public InventoryContainer Hotbar => hotbar;
    public InventoryContainer Backpack => backpack;
    public int SelectedHotbarIndex => selectedHotbarIndex;

    private void Awake()
    {
        //playerHealth = GetComponent<PlayerHealth>();
        //playerEnergy = GetComponent<PlayerEnergy>();

        hotbar.Initialize(hotbarSize);
        backpack.Initialize(backpackSize);

        hotbar.OnChanged += HandleContainerChanged;
        backpack.OnChanged += HandleContainerChanged;

        selectedHotbarIndex = Mathf.Clamp(selectedHotbarIndex, 0, Mathf.Max(0, hotbarSize - 1));

        if (itemUseService == null)
        {
            itemUseService = GetComponent<PlayerItemActions>();
        }
    }

    private void OnDestroy()
    {
        hotbar.OnChanged -= HandleContainerChanged;
        backpack.OnChanged -= HandleContainerChanged;
    }

    public bool TryAddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        if (!CanAddItem(item, amount))
        {
            return false;
        }

        int left = hotbar.AddItem(item, amount);
        left = backpack.AddItem(item, left);

        return left == 0;
    }

    public int AddItemAndReturnLeftover(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return amount;
        }

        int left = hotbar.AddItem(item, amount);
        left = backpack.AddItem(item, left);

        return left;
    }

    public bool CanAddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        InventoryContainer tempHotbar = CloneContainer(hotbar);
        InventoryContainer tempBackpack = CloneContainer(backpack);

        int left = tempHotbar.AddItem(item, amount);
        left = tempBackpack.AddItem(item, left);

        return left == 0;
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= hotbar.SlotCount)
        {
            return;
        }

        if (selectedHotbarIndex == index)
        {
            return;
        }

        selectedHotbarIndex = index;
        OnSelectedHotbarChanged?.Invoke(selectedHotbarIndex);
    }

    public InventorySlot GetSelectedSlot()
    {
        return hotbar.GetSlot(selectedHotbarIndex);
    }

    public ItemData GetSelectedItem()
    {
        InventorySlot slot = GetSelectedSlot();
        return slot == null || slot.IsEmpty ? null : slot.Item;
    }

    public bool UseSelectedItem()
{
    InventorySlot slot = GetSelectedSlot();

    if (slot == null || slot.IsEmpty)
    {
        return false;
    }

    if (itemUseService == null)
    {
        Debug.LogWarning("PlayerInventory: falta asignar PlayerItemActions.");
        return false;
    }

    bool used = itemUseService.TryUse(slot.Item, gameObject);

    if (!used)
    {
        return false;
    }

    if (slot.Item.Stackable)
    {
        hotbar.RemoveFromSlot(selectedHotbarIndex, 1);
    }

    return true;
}

    public bool DropSelectedItem(int amount = 1)
    {
        InventorySlot slot = GetSelectedSlot();

        if (slot == null || slot.IsEmpty)
        {
            return false;
        }

        if (itemDropSpawner == null)
        {
            Debug.LogWarning("PlayerInventory: falta asignar ItemDropSpawner.");
            return false;
        }

        int amountToDrop = Mathf.Clamp(amount, 1, slot.Amount);

        Vector2 dropDirection = transform.right;

        DroppedItem dropped = itemDropSpawner.Spawn(
            slot.Item,
            amountToDrop,
            GetDropPosition(),
            dropDirection
        );

        if (dropped == null)
        {
            return false;
        }

        hotbar.RemoveFromSlot(selectedHotbarIndex, amountToDrop);
        return true;
    }

    public bool MoveHotbarToBackpack(int hotbarIndex, int amount = 0)
    {
        return hotbar.MoveSlotTo(backpack, hotbarIndex, amount);
    }

    public bool MoveBackpackToHotbar(int backpackIndex, int amount = 0)
    {
        return backpack.MoveSlotTo(hotbar, backpackIndex, amount);
    }

    public void SwapHotbarSlots(int a, int b)
    {
        hotbar.SwapSlots(a, b);
    }

    public void SwapBackpackSlots(int a, int b)
    {
        backpack.SwapSlots(a, b);
    }

    public bool RemoveItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        int total = hotbar.CountItem(item) + backpack.CountItem(item);
        if (total < amount)
        {
            return false;
        }

        int left = amount;

        int removeFromHotbar = Mathf.Min(hotbar.CountItem(item), left);
        if (removeFromHotbar > 0)
        {
            hotbar.RemoveItem(item, removeFromHotbar);
            left -= removeFromHotbar;
        }

        if (left > 0)
        {
            backpack.RemoveItem(item, left);
        }

        return true;
    }

    public int CountItem(ItemData item)
    {
        if (item == null)
        {
            return 0;
        }

        return hotbar.CountItem(item) + backpack.CountItem(item);
    }

    public bool HasItem(ItemData item, int amount)
    {
        return CountItem(item) >= amount;
    }

    private void ConsumeItem(ConsumableItemData consumable)
    {
        /*
        if (playerHealth != null && consumable.HealthRestore > 0)
        {
            playerHealth.Heal(consumable.HealthRestore);
        }

        
        if (playerEnergy != null && consumable.EnergyRestore > 0)
        {
            playerEnergy.RestoreEnergy(consumable.EnergyRestore);
        }*/
    }

    private Vector3 GetDropPosition()
    {
        if (dropPoint != null)
        {
            return dropPoint.position;
        }

        return transform.position + transform.right * dropDistance;
    }

    private void HandleContainerChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    private InventoryContainer CloneContainer(InventoryContainer source)
    {
        InventoryContainer clone = new InventoryContainer(source.SlotCount);
        
        for (int i = 0; i < source.SlotCount; i++)
        {
            InventorySlot sourceSlot = source.GetSlot(i);
            InventorySlot cloneSlot = clone.GetSlot(i);

            if (sourceSlot == null || sourceSlot.IsEmpty)
            {
                continue;
            }

            cloneSlot.Item = sourceSlot.Item;
            cloneSlot.Amount = sourceSlot.Amount;
        }

        return clone;
    }
}
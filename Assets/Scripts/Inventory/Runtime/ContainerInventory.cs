using System;
using UnityEngine;

public abstract class ContainerInventory : MonoBehaviour
{
    public event Action OnContainerChanged;

    [Header("Container")]
    [SerializeField] private int slotCount = 12;
    [SerializeField] private InventoryContainer container = new InventoryContainer();

    public InventoryContainer Container => container;
    public int SlotCount => slotCount;

    protected virtual void Awake()
    {
        container.Initialize(slotCount);
        container.OnChanged += HandleContainerChanged;
    }

    protected virtual void OnDestroy()
    {
        container.OnChanged -= HandleContainerChanged;
    }

    private void HandleContainerChanged()
    {
        OnContainerChanged?.Invoke();
    }
}
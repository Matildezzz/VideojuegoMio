using UnityEngine;

public sealed class ChestInventory : ContainerInventory
{
    [Header("Chest")]
    [SerializeField] private string chestId;

    public string ChestId => chestId;

    public bool IsOpen { get; private set; }

    public void Open()
    {
        IsOpen = true;
    }

    public void Close()
    {
        IsOpen = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(chestId))
        {
            chestId = System.Guid.NewGuid().ToString();
        }
    }

    [ContextMenu("Generate New Chest Id")]
    private void GenerateNewChestId()
    {
        chestId = System.Guid.NewGuid().ToString();
    }
#endif
}
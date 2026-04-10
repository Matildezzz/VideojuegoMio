using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumableItem", menuName = "Gameplay/Items/Consumable Item")]
public class ConsumableItemData : ItemData
{
    [Header("Consumable Stats")]
    [SerializeField] private int healthRestore;
    [SerializeField] private int energyRestore;

    public int HealthRestore => healthRestore;
    public int EnergyRestore => energyRestore;
}
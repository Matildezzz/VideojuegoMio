using UnityEngine;

[CreateAssetMenu(fileName = "NewToolItem", menuName = "Gameplay/Items/Tool Item")]
public class ToolItemData : ItemData
{
    [Header("Tool Data")]
    [SerializeField] private ToolType toolType = ToolType.None;
    [SerializeField] private int energyCost = 1;

    [Header("Weapon Data")]
    [SerializeField] private int weaponDamage = 1;
    [SerializeField] private float weaponRange = 0.65f;
    [SerializeField] private float weaponCooldown = 0.35f;

    public ToolType ToolType => toolType;
    public int EnergyCost => energyCost;
    public int WeaponDamage => weaponDamage;
    public float WeaponRange => weaponRange;
    public float WeaponCooldown => weaponCooldown;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (energyCost < 0)
        {
            energyCost = 0;
        }

        if (weaponDamage < 1)
        {
            weaponDamage = 1;
        }

        if (weaponRange < 0.05f)
        {
            weaponRange = 0.05f;
        }

        if (weaponCooldown < 0.01f)
        {
            weaponCooldown = 0.01f;
        }
    }
#endif
}
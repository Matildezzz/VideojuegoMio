using UnityEngine;

[CreateAssetMenu(fileName = "NewToolItem", menuName = "Gameplay/Items/Tool Item")]
public class ToolItemData : ItemData
{
    [Header("Tool Data")]
    [SerializeField] private ToolType toolType = ToolType.None;

    public ToolType ToolType => toolType;
}
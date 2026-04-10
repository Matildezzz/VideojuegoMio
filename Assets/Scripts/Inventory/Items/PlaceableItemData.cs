using UnityEngine;

[CreateAssetMenu(fileName = "NewPlaceableItem", menuName = "Gameplay/Items/Placeable Item")]
public class PlaceableItemData : ItemData
{
    [Header("Placeable Data")]
    [SerializeField] private GameObject placedPrefab;

    public GameObject PlacedPrefab => placedPrefab;
}
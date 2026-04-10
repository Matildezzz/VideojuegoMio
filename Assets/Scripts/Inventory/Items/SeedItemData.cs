using UnityEngine;

[CreateAssetMenu(fileName = "NewSeedItem", menuName = "Gameplay/Items/Seed Item")]
public class SeedItemData : ItemData
{
    [Header("Seed Data")]
    [SerializeField] private GameObject cropPrefab;

    public GameObject CropPrefab => cropPrefab;
}
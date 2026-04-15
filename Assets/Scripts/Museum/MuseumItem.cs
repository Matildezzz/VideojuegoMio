using UnityEngine;

[CreateAssetMenu(fileName = "MuseumItem", menuName = "Gameplay/Items/Museum Item")]
public class MuseumItemData : ItemData
{
    [Header("Museo")]
    [SerializeField] private string museumDisplayName;
    [SerializeField] private string description;
    [SerializeField] private int donationRewardGold = 0;

    public string MuseumDisplayName => museumDisplayName;
    public string Description => description;
    public int DonationRewardGold => donationRewardGold;
}
using UnityEngine;

[CreateAssetMenu(fileName = "MuseumItem", menuName = "Gameplay/Items/Museum Item")]
public class MuseumItemData : ItemData
{
    [Header("Museo")]
    [SerializeField] private string museumDisplayName;
    [SerializeField] private string museumDescription;
    [SerializeField] private int donationRewardGold = 0;

    public string MuseumDisplayName => museumDisplayName;
    public string MuseumDescription => museumDescription;
    public int DonationRewardGold => donationRewardGold;
}
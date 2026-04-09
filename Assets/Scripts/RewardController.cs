using NUnit.Framework;
using UnityEngine;

public class RewardController : MonoBehaviour
{
    public static RewardController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GiveQuestReward(Quest quest)
    {
        if (quest?.questRewards == null) return;

        foreach (var reward in quest.questRewards)
        {
            switch (reward.type)
            {
                case RewardType.Item:
                    GiveItemReward(reward.rewardID, reward.amount);
                    break;
                case RewardType.Gold:
                    break;
                case RewardType.Experience:
                    break;
                case RewardType.Custom:
                    break;
            }
        }
    }

    public void GiveItemReward(int itemID, int amount)
    {
        GameObject itemPrefab = FindAnyObjectByType<ItemDictionary>()?.GetItemPrefab(itemID);

        if (itemPrefab == null)
        {
            return;
        }

        Item item = itemPrefab.GetComponent<Item>();
        GameObject worldPrefab = item != null ? item.GetWorldPrefab() : itemPrefab;

        for (int i = 0; i < amount; i++)
        {
            if (!InventoryController.Instance.AddItem(itemPrefab))
            {
                GameObject dropItem = Instantiate(worldPrefab, transform.position + Vector3.down, Quaternion.identity);

                BounceEffect bounce = dropItem.GetComponent<BounceEffect>();
                if (bounce != null)
                {
                    bounce.StartBounce();
                }
            }
            else
            {
                if (item != null)
                {
                    item.ShowPopUp();
                }
            }
        }
    }
}
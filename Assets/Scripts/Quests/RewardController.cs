using UnityEngine;

public class RewardController : MonoBehaviour
{
    public static RewardController Instance { get; private set; }

    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private ItemDropSpawner itemDropSpawner;
    [SerializeField] private Transform rewardDropOrigin;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }

        if (itemDropSpawner == null)
        {
            itemDropSpawner = FindAnyObjectByType<ItemDropSpawner>();
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("RewardController: falta asignar ItemDatabase.");
        }
    }

    public void GiveQuestReward(Quest quest)
    {
        if (quest == null || quest.questRewards == null)
        {
            return;
        }

        foreach (var reward in quest.questRewards)
        {
            switch (reward.type)
            {
                case RewardType.Item:
                    GiveItemReward(reward.rewardItemId, reward.amount);
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

    public void GiveItemReward(string itemId, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("RewardController: no hay ItemDatabase.");
            return;
        }

        ItemData itemData = itemDatabase.GetItemById(itemId);
        if (itemData == null)
        {
            Debug.LogWarning("RewardController: rewardItemId no existe -> " + itemId);
            return;
        }

        if (playerInventory == null)
        {
            DropRewardToWorld(itemData, amount);
            return;
        }

        int leftover = playerInventory.AddItemAndReturnLeftover(itemData, amount);
        int receivedAmount = amount - leftover;

        if (receivedAmount > 0 && ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("+" + receivedAmount + " " + itemData.DisplayName, ToastType.Success, "Pickup");
        }

        if (leftover > 0)
        {
            DropRewardToWorld(itemData, leftover);

            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("Inventario lleno: parte de la recompensa cayó al suelo", ToastType.Warning, "Error");
            }
        }
    }

    private void DropRewardToWorld(ItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0)
        {
            return;
        }

        Vector3 origin = rewardDropOrigin != null
            ? rewardDropOrigin.position
            : playerInventory != null
                ? playerInventory.transform.position
                : transform.position;

        Vector2 direction = playerInventory != null
            ? (Vector2)playerInventory.transform.right
            : Vector2.down;

        if (itemDropSpawner != null)
        {
            itemDropSpawner.Spawn(itemData, amount, origin, direction);
            return;
        }

        if (itemData.WorldPrefab == null)
        {
            Debug.LogWarning("RewardController: el item no tiene WorldPrefab -> " + itemData.DisplayName);
            return;
        }

        GameObject instance = Instantiate(itemData.WorldPrefab, origin, Quaternion.identity);
        DroppedItem droppedItem = instance.GetComponent<DroppedItem>();

        if (droppedItem != null)
        {
            droppedItem.Setup(itemData, amount);
        }
    }
}
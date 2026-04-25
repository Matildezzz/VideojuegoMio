using System.Collections.Generic;
using UnityEngine;

public class RewardController : MonoBehaviour
{
    public static RewardController Instance { get; private set; }

    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private ItemDropSpawner itemDropSpawner;
    [SerializeField] private Transform rewardDropOrigin;
    [SerializeField] private PlayerExperienceController playerExperience;

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

        if (playerExperience == null)
        {
            playerExperience = FindAnyObjectByType<PlayerExperienceController>();
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("RewardController: falta asignar ItemDatabase.");
        }
    }

    public bool GiveQuestReward(Quest quest)
    {
        if (quest == null || quest.questRewards == null || quest.questRewards.Count == 0)
        {
            return false;
        }

        bool gaveAnyReward = false;
        List<string> rewardMessages = new List<string>();

        foreach (QuestReward reward in quest.questRewards)
        {
            if (reward == null)
            {
                continue;
            }

            switch (reward.type)
            {
                case RewardType.Item:
                {
                    string itemMessage = GiveItemRewardInternal(reward.rewardItemId, reward.amount, false);
                    if (!string.IsNullOrWhiteSpace(itemMessage))
                    {
                        rewardMessages.Add(itemMessage);
                        gaveAnyReward = true;
                    }
                    break;
                }

                case RewardType.Gold:
                {
                    if (GiveGoldReward(reward.amount, false))
                    {
                        rewardMessages.Add("+" + reward.amount + " oro");
                        gaveAnyReward = true;
                    }
                    break;
                }

                case RewardType.Experience:
                {
                    if (GiveExperienceReward(reward.amount, false))
                    {
                        rewardMessages.Add("+" + reward.amount + " EXP");
                        gaveAnyReward = true;
                    }
                    break;
                }

                case RewardType.Custom:
                    Debug.Log("RewardController: recompensa Custom pendiente de implementar en la mision " + quest.questName + ".");
                    break;
            }
        }

        if (gaveAnyReward && rewardMessages.Count > 0 && ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("Recompensas: " + string.Join("  |  ", rewardMessages), ToastType.Success, "Coin");
        }

        return gaveAnyReward;
    }

    public void GiveItemReward(string itemId, int amount)
    {
        GiveItemRewardInternal(itemId, amount, true);
    }

    public void GiveGoldReward(int amount)
    {
        GiveGoldReward(amount, true);
    }

    public void GiveExperienceReward(int amount)
    {
        GiveExperienceReward(amount, true);
    }

    private string GiveItemRewardInternal(string itemId, int amount, bool showToast)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return string.Empty;
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("RewardController: no hay ItemDatabase.");
            return string.Empty;
        }

        ItemData itemData = itemDatabase.GetItemById(itemId);
        if (itemData == null)
        {
            Debug.LogWarning("RewardController: rewardItemId no existe -> " + itemId);
            return string.Empty;
        }

        int leftover = amount;

        if (playerInventory != null)
        {
            leftover = playerInventory.AddItemAndReturnLeftover(itemData, amount);
        }

        int receivedInInventory = amount - leftover;

        if (leftover > 0)
        {
            DropRewardToWorld(itemData, leftover);

            if (ToastManager.Instance != null)
            {
                string warning = receivedInInventory > 0
                    ? "Inventario lleno: parte de la recompensa cayo al suelo"
                    : "Inventario lleno: la recompensa cayo al suelo";

                ToastManager.Instance.ShowToast(warning, ToastType.Warning, "Error");
            }
        }

        string message = "+" + amount + " " + itemData.DisplayName;

        if (showToast && ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Success, "Pickup");
        }

        return message;
    }

    private bool GiveGoldReward(int amount, bool showToast)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (CurrencyController.Instance == null)
        {
            Debug.LogWarning("RewardController: no hay CurrencyController en la escena. No se pudo entregar oro.");
            return false;
        }

        CurrencyController.Instance.AddGold(amount);

        if (showToast && ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("+" + amount + " oro", ToastType.Success, "Coin");
        }

        return true;
    }

    private bool GiveExperienceReward(int amount, bool showToast)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (playerExperience == null)
        {
            playerExperience = FindAnyObjectByType<PlayerExperienceController>();
        }

        if (playerExperience == null)
        {
            Debug.LogWarning("RewardController: no hay PlayerExperienceController en la escena. Anade ese script al Player o a un GameObject para usar recompensas de experiencia.");
            return false;
        }

        playerExperience.AddExperience(amount);

        if (showToast && ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("+" + amount + " EXP", ToastType.Success, "QuestComplete");
        }

        return true;
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

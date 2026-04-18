using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }

    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private QuestUI questUI;

    public List<QuestProgress> activeQuests = new();
    public List<string> handinQuestIDs = new();

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

        if (questUI == null)
        {
            questUI = FindAnyObjectByType<QuestUI>();
        }

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += CheckInventoryForQuests;
        }
        else
        {
            Debug.LogWarning("QuestController: falta asignar PlayerInventory.");
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("QuestController: falta asignar ItemDatabase.");
        }
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= CheckInventoryForQuests;
        }
    }

    public void AcceptQuest(Quest quest)
    {
        if (quest == null || IsQuestActive(quest.questID))
        {
            return;
        }

        activeQuests.Add(new QuestProgress(quest));
        CheckInventoryForQuests();
        questUI?.UpdateQuestUI();
    }

    public bool IsQuestActive(string questID)
    {
        return activeQuests.Exists(q => q.QuestID == questID);
    }

    public void CheckInventoryForQuests()
    {
        if (playerInventory == null || itemDatabase == null)
        {
            return;
        }

        foreach (QuestProgress quest in activeQuests)
        {
            foreach (QuestObjective questObjective in quest.objectives)
            {
                if (questObjective.type != ObjectiveType.CollectItem)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(questObjective.objectiveID))
                {
                    questObjective.currentAmount = 0;
                    continue;
                }

                ItemData itemData = itemDatabase.GetItemById(questObjective.objectiveID);
                int count = itemData != null ? playerInventory.CountItem(itemData) : 0;

                questObjective.currentAmount = Mathf.Min(count, questObjective.requiredAmount);
            }
        }

        questUI?.UpdateQuestUI();
    }

    public bool IsQuestCompleted(string questID)
    {
        QuestProgress quest = activeQuests.Find(q => q.QuestID == questID);
        return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    }

    public void HandInQuest(string questID)
    {
        if (!RemoveRequiredItemsFromInventory(questID))
        {
            return;
        }

        QuestProgress quest = activeQuests.Find(q => q.QuestID == questID);
        if (quest != null)
        {
            handinQuestIDs.Add(questID);
            activeQuests.Remove(quest);
            questUI?.UpdateQuestUI();
        }
    }

    public bool IsQuestHandedIn(string questID)
    {
        return handinQuestIDs.Contains(questID);
    }

    public bool RemoveRequiredItemsFromInventory(string questID)
    {
        if (playerInventory == null || itemDatabase == null)
        {
            return false;
        }

        QuestProgress quest = activeQuests.Find(q => q.QuestID == questID);
        if (quest == null)
        {
            return false;
        }

        Dictionary<ItemData, int> requiredItems = new();

        foreach (QuestObjective objective in quest.objectives)
        {
            if (objective.type != ObjectiveType.CollectItem)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(objective.objectiveID))
            {
                continue;
            }

            ItemData itemData = itemDatabase.GetItemById(objective.objectiveID);
            if (itemData == null)
            {
                Debug.LogWarning("QuestController: objectiveID no existe en ItemDatabase -> " + objective.objectiveID);
                return false;
            }

            if (!requiredItems.ContainsKey(itemData))
            {
                requiredItems[itemData] = 0;
            }

            requiredItems[itemData] += objective.requiredAmount;
        }

        foreach (var itemRequirement in requiredItems)
        {
            if (!playerInventory.HasItem(itemRequirement.Key, itemRequirement.Value))
            {
                return false;
            }
        }

        foreach (var itemRequirement in requiredItems)
        {
            playerInventory.RemoveItem(itemRequirement.Key, itemRequirement.Value);
        }

        return true;
    }

    public void LoadQuestProgress(List<QuestProgress> savedQuests)
    {
        activeQuests = savedQuests ?? new List<QuestProgress>();
        CheckInventoryForQuests();
        questUI?.UpdateQuestUI();
    }

    public void RegisterEnemyDefeated(string enemyId)
    {
        if (string.IsNullOrWhiteSpace(enemyId))
        {
            return;
        }

        foreach (QuestProgress quest in activeQuests)
        {
            foreach (QuestObjective questObjective in quest.objectives)
            {
                if (questObjective.type != ObjectiveType.DefeatEnemy)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(questObjective.objectiveID) &&
                    questObjective.objectiveID != enemyId)
                {
                    continue;
                }

                questObjective.currentAmount = Mathf.Min(
                    questObjective.currentAmount + 1,
                    questObjective.requiredAmount
                );
            }
        }

        questUI?.UpdateQuestUI();
    }
}
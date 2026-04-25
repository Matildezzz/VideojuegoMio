using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }

    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private QuestUI questUI;
    [SerializeField] private QuestHUDUI questHUDUI;
    [SerializeField] private QuestBannerUI questBannerUI;

    public List<QuestProgress> activeQuests = new();
    public List<string> handinQuestIDs = new();

    public event Action OnQuestUpdated;

    private readonly Dictionary<string, QuestNpcInfo> questNpcInfos = new();
    private string followedQuestID;

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

        if (questHUDUI == null)
        {
            questHUDUI = FindAnyObjectByType<QuestHUDUI>();
        }

        if (questBannerUI == null)
        {
            questBannerUI = FindAnyObjectByType<QuestBannerUI>();
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

    private void Start()
    {
        NotifyQuestUI();
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
        if (quest == null || IsQuestActive(quest.questID) || IsQuestHandedIn(quest.questID))
        {
            return;
        }

        QuestProgress progress = new QuestProgress(quest);
        activeQuests.Add(progress);

        if (string.IsNullOrWhiteSpace(followedQuestID) || quest.followOnAccept)
        {
            followedQuestID = quest.questID;
        }

        CheckInventoryForQuests(false);
        RefreshQuestState(progress, false);
        NotifyQuestUI();

        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("Nueva misión: " + quest.questName, ToastType.Success, "QuestComplete");
        }
    }

    public bool IsQuestActive(string questID)
    {
        return activeQuests.Exists(q => q.QuestID == questID);
    }

    public QuestProgress GetQuestProgress(string questID)
    {
        if (string.IsNullOrWhiteSpace(questID))
        {
            return null;
        }

        return activeQuests.Find(q => q.QuestID == questID);
    }

    public QuestProgress GetFollowedQuest()
    {
        QuestProgress followedQuest = GetQuestProgress(followedQuestID);

        if (followedQuest != null)
        {
            return followedQuest;
        }

        followedQuest = GetFirstActiveQuest();
        followedQuestID = followedQuest != null ? followedQuest.QuestID : string.Empty;
        return followedQuest;
    }

    public QuestProgress GetFirstActiveQuest()
    {
        if (activeQuests == null || activeQuests.Count == 0)
        {
            return null;
        }

        return activeQuests[0];
    }

    public string GetFollowedQuestID()
    {
        return followedQuestID;
    }

    public bool IsFollowedQuest(string questID)
    {
        return !string.IsNullOrWhiteSpace(questID) && questID == followedQuestID;
    }

    public void FollowQuest(string questID)
    {
        if (string.IsNullOrWhiteSpace(questID) || GetQuestProgress(questID) == null)
        {
            return;
        }

        followedQuestID = questID;
        NotifyQuestUI();

        QuestProgress quest = GetQuestProgress(questID);
        if (quest != null && ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("Siguiendo misión: " + quest.quest.questName, ToastType.Normal);
        }
    }

    public void CheckInventoryForQuests()
    {
        CheckInventoryForQuests(true);
    }

    private void CheckInventoryForQuests(bool notifyCompleted)
    {
        if (playerInventory == null || itemDatabase == null)
        {
            NotifyQuestUI();
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

            RefreshQuestState(quest, notifyCompleted);
        }

        NotifyQuestUI();
    }

    private void RefreshQuestState(QuestProgress quest, bool notifyCompleted)
    {
        if (quest == null || quest.state == QuestState.HandedIn)
        {
            return;
        }

        bool wasActive = quest.state == QuestState.Active;
        quest.state = quest.IsCompleted ? QuestState.Completed : QuestState.Active;

        if (notifyCompleted && wasActive && quest.state == QuestState.Completed)
        {
            ShowQuestReadyFeedback(quest);
        }
    }

    private void ShowQuestReadyFeedback(QuestProgress quest)
    {
        if (quest == null || quest.quest == null)
        {
            return;
        }

        string title = "Misión lista para entregar";
        string subtitle = GetHandInHint(quest);
        Sprite icon = GetHandInIcon(quest);

        if (questBannerUI != null)
        {
            questBannerUI.ShowBanner(title, subtitle, icon);
        }

        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(title + ": " + quest.quest.questName, ToastType.Success, "QuestComplete");
        }
    }

    public bool IsQuestCompleted(string questID)
    {
        QuestProgress quest = activeQuests.Find(q => q.QuestID == questID);
        return quest != null && quest.IsCompleted;
    }

    public QuestState GetQuestState(string questID)
    {
        if (IsQuestHandedIn(questID))
        {
            return QuestState.HandedIn;
        }

        QuestProgress quest = GetQuestProgress(questID);
        return quest != null ? quest.state : QuestState.HandedIn;
    }

    public bool HandInQuest(string questID)
    {
        QuestProgress quest = activeQuests.Find(q => q.QuestID == questID);

        if (quest == null)
        {
            return false;
        }

        if (!RemoveRequiredItemsFromInventory(questID))
        {
            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("No tienes los objetos necesarios para entregar la misión.", ToastType.Error, "Error");
            }
            return false;
        }

        bool gaveRewards = false;
        if (RewardController.Instance != null)
        {
            gaveRewards = RewardController.Instance.GiveQuestReward(quest.quest);
        }
        else
        {
            Debug.LogWarning("QuestController: no hay RewardController en la escena. La misión se entregará sin recompensas.");
        }

        quest.state = QuestState.HandedIn;
        handinQuestIDs.Add(questID);
        activeQuests.Remove(quest);

        if (followedQuestID == questID)
        {
            QuestProgress nextQuest = GetFirstActiveQuest();
            followedQuestID = nextQuest != null ? nextQuest.QuestID : string.Empty;
        }

        NotifyQuestUI();

        if (questBannerUI != null)
        {
            questBannerUI.ShowBanner("Misión completada", quest.quest.questName, GetHandInIcon(quest));
        }

        if (!gaveRewards && ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("Misión completada: " + quest.quest.questName, ToastType.Success, "QuestComplete");
        }

        return true;
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

        foreach (QuestProgress quest in activeQuests)
        {
            RefreshQuestState(quest, false);
        }

        if (string.IsNullOrWhiteSpace(followedQuestID) && activeQuests.Count > 0)
        {
            followedQuestID = activeQuests[0].QuestID;
        }

        CheckInventoryForQuests(false);
        NotifyQuestUI();
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

            RefreshQuestState(quest, true);
        }

        NotifyQuestUI();
    }

    public void RegisterQuestNpcInfo(Quest quest, string npcName, Sprite npcIcon)
    {
        if (quest == null || string.IsNullOrWhiteSpace(quest.questID))
        {
            return;
        }

        if (!questNpcInfos.ContainsKey(quest.questID))
        {
            questNpcInfos.Add(quest.questID, new QuestNpcInfo());
        }

        questNpcInfos[quest.questID].npcName = npcName;
        questNpcInfos[quest.questID].npcIcon = npcIcon;
        NotifyQuestUI();
    }

    public string GetHandInHint(QuestProgress quest)
    {
        if (quest == null || quest.quest == null)
        {
            return "Vuelve con el NPC de la misión.";
        }

        if (!string.IsNullOrWhiteSpace(quest.quest.handInHint))
        {
            return quest.quest.handInHint;
        }

        if (questNpcInfos.TryGetValue(quest.QuestID, out QuestNpcInfo info) && !string.IsNullOrWhiteSpace(info.npcName))
        {
            return "Vuelve con " + info.npcName + ".";
        }

        return "Vuelve con quien te dio la misión.";
    }

    public Sprite GetHandInIcon(QuestProgress quest)
    {
        if (quest == null || quest.quest == null)
        {
            return null;
        }

        if (quest.quest.handInNpcIcon != null)
        {
            return quest.quest.handInNpcIcon;
        }

        if (questNpcInfos.TryGetValue(quest.QuestID, out QuestNpcInfo info))
        {
            return info.npcIcon;
        }

        return null;
    }

    private void NotifyQuestUI()
    {
        questUI?.UpdateQuestUI();
        questHUDUI?.UpdateQuestHUD();
        OnQuestUpdated?.Invoke();
    }

    private class QuestNpcInfo
    {
        public string npcName;
        public Sprite npcIcon;
    }
}

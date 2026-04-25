using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Quest")]
public class Quest : ScriptableObject
{
    [Header("Datos principales")]
    public string questID;
    public string questName;
    [TextArea(2, 4)] public string description;

    [Header("Diario / HUD")]
    [Tooltip("Si está activo, esta misión se marcará como misión seguida al aceptarla.")]
    public bool followOnAccept = true;

    [Tooltip("Texto que se muestra en el HUD cuando la misión ya se puede entregar.")]
    public string handInHint;

    [Tooltip("Icono opcional del NPC que recibe la misión. Si se deja vacío, se usará el retrato del NPC del diálogo cuando exista.")]
    public Sprite handInNpcIcon;

    [Header("Objetivos y recompensas")]
    public List<QuestObjective> objectives;
    public List<QuestReward> questRewards;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + Guid.NewGuid().ToString();
        }
    }
}

[Serializable]
public class QuestObjective
{
    public string objectiveID;
    public string description;
    public ObjectiveType type;
    public int requiredAmount;
    public int currentAmount;

    public bool IsCompleted => currentAmount >= requiredAmount;
}

public enum ObjectiveType
{
    CollectItem,
    DefeatEnemy,
    ReachLocation,
    TalkNPC,
    Custom
}

public enum QuestState
{
    Active,
    Completed,
    HandedIn
}

[Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<QuestObjective> objectives;
    public QuestState state = QuestState.Active;

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        state = QuestState.Active;
        objectives = new List<QuestObjective>();

        foreach (var obj in quest.objectives)
        {
            objectives.Add(new QuestObjective
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0
            });
        }
    }

    public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted);
    public string QuestID => quest != null ? quest.questID : string.Empty;

    public QuestObjective GetCurrentObjective()
    {
        if (objectives == null || objectives.Count == 0)
        {
            return null;
        }

        QuestObjective firstIncomplete = objectives.Find(o => !o.IsCompleted);
        return firstIncomplete ?? objectives[objectives.Count - 1];
    }
}

[Serializable]
public class QuestReward
{
    public RewardType type;
    public string rewardItemId;
    public int amount = 1;
}

public enum RewardType
{
    Item,
    Gold,
    Experience,
    Custom
}

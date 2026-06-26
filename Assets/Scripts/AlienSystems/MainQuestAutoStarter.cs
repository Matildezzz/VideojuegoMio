using System.Collections;
using UnityEngine;

public sealed class MainQuestAutoStarter : MonoBehaviour
{
    [Header("Mision principal")]
    [SerializeField] private Quest mainQuest;
    [SerializeField] private string firstObjectiveID = "wake_up";
    [SerializeField] private bool completeFirstObjectiveOnStart = true;

    private IEnumerator Start()
    {
        yield return null;

        if (mainQuest == null || QuestController.Instance == null)
        {
            yield break;
        }

        if (!QuestController.Instance.IsQuestActive(mainQuest.questID) && !QuestController.Instance.IsQuestHandedIn(mainQuest.questID))
        {
            QuestController.Instance.AcceptQuest(mainQuest);
        }

        if (completeFirstObjectiveOnStart && !string.IsNullOrWhiteSpace(firstObjectiveID))
        {
            QuestController.Instance.RegisterObjectiveProgress(ObjectiveType.Custom, firstObjectiveID, 1);
        }
    }
}

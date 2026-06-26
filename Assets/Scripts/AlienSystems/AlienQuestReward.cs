using UnityEngine;

public sealed class AlienQuestReward : MonoBehaviour
{
    [SerializeField] private string questID;
    [SerializeField] private string[] wordsToLearn;
    [SerializeField] private string[] itemNameIdsToLearn;
    [SerializeField] private string[] characterNameIdsToLearn;
    [SerializeField] private AlienDiaryEntry[] diaryEntriesToUnlock;

    private void OnEnable()
    {
        if (QuestController.Instance != null)
        {
            QuestController.Instance.OnQuestHandedIn += HandleQuestHandedIn;
        }
    }

    private void Start()
    {
        if (QuestController.Instance != null)
        {
            QuestController.Instance.OnQuestHandedIn -= HandleQuestHandedIn;
            QuestController.Instance.OnQuestHandedIn += HandleQuestHandedIn;
        }
    }

    private void OnDisable()
    {
        if (QuestController.Instance != null)
        {
            QuestController.Instance.OnQuestHandedIn -= HandleQuestHandedIn;
        }
    }

    private void HandleQuestHandedIn(string completedQuestID)
    {
        if (string.IsNullOrWhiteSpace(questID) || completedQuestID != questID)
        {
            return;
        }

        LanguageManager.Instance?.LearnWords(wordsToLearn);
        AlienNameManager.Instance?.LearnItemNames(itemNameIdsToLearn);

        if (AlienNameManager.Instance != null && characterNameIdsToLearn != null)
        {
            for (int i = 0; i < characterNameIdsToLearn.Length; i++)
            {
                AlienNameManager.Instance.LearnCharacterName(characterNameIdsToLearn[i]);
            }
        }

        if (AlienDiaryManager.Instance != null && diaryEntriesToUnlock != null)
        {
            for (int i = 0; i < diaryEntriesToUnlock.Length; i++)
            {
                AlienDiaryEntry entry = diaryEntriesToUnlock[i];
                if (entry == null)
                {
                    continue;
                }

                AlienDiaryManager.Instance.UnlockEntry(entry.entryId, entry.title, entry.content, entry.category);
            }
        }
    }
}

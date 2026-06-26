using UnityEngine;

public sealed class AlienProgressTrigger : MonoBehaviour, IInteractable
{
    [Header("Interaccion")]
    [SerializeField] private string interactionText = "E - Observar";
    [SerializeField] private bool disableAfterUse;

    [Header("Mision")]
    [SerializeField] private ObjectiveType objectiveType = ObjectiveType.Custom;
    [SerializeField] private string objectiveID;
    [SerializeField] private int objectiveAmount = 1;

    [Header("Idioma")]
    [SerializeField] private string[] wordsToLearn;

    [Header("Nombres")]
    [SerializeField] private string[] itemNameIdsToLearn;
    [SerializeField] private string[] characterNameIdsToLearn;

    [Header("Diario")]
    [SerializeField] private AlienDiaryEntry[] diaryEntriesToUnlock;

    public string InteractionText => interactionText;

    public bool CanInteract()
    {
        return enabled;
    }

    public void Interact()
    {
        if (!string.IsNullOrWhiteSpace(objectiveID) && QuestController.Instance != null)
        {
            QuestController.Instance.RegisterObjectiveProgress(objectiveType, objectiveID, Mathf.Max(1, objectiveAmount));
        }

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.LearnWords(wordsToLearn);
        }

        if (AlienNameManager.Instance != null)
        {
            AlienNameManager.Instance.LearnItemNames(itemNameIdsToLearn);

            if (characterNameIdsToLearn != null)
            {
                for (int i = 0; i < characterNameIdsToLearn.Length; i++)
                {
                    AlienNameManager.Instance.LearnCharacterName(characterNameIdsToLearn[i]);
                }
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

        if (disableAfterUse)
        {
            enabled = false;
        }
    }
}

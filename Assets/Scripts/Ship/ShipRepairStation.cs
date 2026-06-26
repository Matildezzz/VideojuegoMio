using UnityEngine;

public sealed class ShipRepairStation : MonoBehaviour, IInteractable
{
    [Header("Texto")]
    [SerializeField] private string interactionText = "E - Reparar nave";
    [SerializeField] private string repairedText = "El traductor basico ya esta reparado.";

    [Header("Requisito")]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private string requiredItemId = "translatorFragment";
    [SerializeField] private int requiredAmount = 1;

    [Header("Observacion inicial")]
    [SerializeField] private bool registerWakeObjectiveOnFirstInteraction = true;
    [SerializeField] private string wakeObjectiveID = "wake_up";
    [SerializeField] private string wakeDiaryEntryId = "ship_crash";
    [SerializeField] private string wakeDiaryTitle = "Casa-cielo rota";
    [TextArea(2, 5)] [SerializeField] private string wakeDiaryContent = "La nave ha dejado de ser nave y ahora es casa rota. El traductor emite sonidos tristes. Necesita piezas.";
    [SerializeField] private string wakeDiaryCategory = "Nave";

    [Header("Mision")]
    [SerializeField] private string questID = "main_algo_cayo_del_cielo";
    [SerializeField] private ObjectiveType objectiveType = ObjectiveType.Custom;
    [SerializeField] private string objectiveID = "repair_translator_1";
    [SerializeField] private int objectiveAmount = 1;
    [SerializeField] private bool handInQuestAfterRepair = true;

    [Header("Progreso alienigena")]
    [SerializeField] private string[] wordsToLearn;
    [SerializeField] private string[] itemNameIdsToLearn;
    [SerializeField] private string[] characterNameIdsToLearn;
    [SerializeField] private AlienDiaryEntry[] diaryEntriesToUnlock;

    public string InteractionText => IsAlreadyRepaired() ? repairedText : interactionText;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        RegisterWakeObjectiveIfNeeded();

        if (IsAlreadyRepaired())
        {
            ShowToast(repairedText, ToastType.Normal, "QuestComplete");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("ShipRepairStation: falta ItemDatabase.");
            ShowToast("La nave no reconoce sus propias piezas.", ToastType.Error, "Error");
            return;
        }

        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("ShipRepairStation: falta PlayerInventory.");
            ShowToast("No se puede acceder al inventario.", ToastType.Error, "Error");
            return;
        }

        ItemData requiredItem = itemDatabase.GetItemById(requiredItemId);
        if (requiredItem == null)
        {
            ShowToast("Falta configurar el material de reparacion.", ToastType.Error, "Error");
            return;
        }

        int amountNeeded = Mathf.Max(1, requiredAmount);
        if (!playerInventory.HasItem(requiredItem, amountNeeded))
        {
            ShowToast("Necesitas " + amountNeeded + " x " + AlienNameManager.GetDisplayName(requiredItem) + ".", ToastType.Warning, "Error");
            return;
        }

        playerInventory.RemoveItem(requiredItem, amountNeeded);

        if (!string.IsNullOrWhiteSpace(objectiveID) && QuestController.Instance != null)
        {
            QuestController.Instance.RegisterObjectiveProgress(objectiveType, objectiveID, Mathf.Max(1, objectiveAmount));
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

        ShowToast("Traductor basico reparado.", ToastType.Success, "QuestComplete");

        if (handInQuestAfterRepair && QuestController.Instance != null && !string.IsNullOrWhiteSpace(questID))
        {
            QuestProgress progress = QuestController.Instance.GetQuestProgress(questID);
            if (progress != null && progress.IsCompleted)
            {
                QuestController.Instance.HandInQuest(questID);
            }
        }
    }

    private void RegisterWakeObjectiveIfNeeded()
    {
        if (!registerWakeObjectiveOnFirstInteraction)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(wakeObjectiveID) && QuestController.Instance != null)
        {
            QuestController.Instance.RegisterObjectiveProgress(ObjectiveType.Custom, wakeObjectiveID, 1);
        }

        AlienDiaryManager.Instance?.UnlockEntry(wakeDiaryEntryId, wakeDiaryTitle, wakeDiaryContent, wakeDiaryCategory);
    }

    private bool IsAlreadyRepaired()
    {
        if (QuestController.Instance == null || string.IsNullOrWhiteSpace(questID))
        {
            return false;
        }

        if (QuestController.Instance.IsQuestHandedIn(questID))
        {
            return true;
        }

        QuestProgress progress = QuestController.Instance.GetQuestProgress(questID);
        if (progress == null || progress.objectives == null)
        {
            return false;
        }

        for (int i = 0; i < progress.objectives.Count; i++)
        {
            QuestObjective objective = progress.objectives[i];
            if (objective == null)
            {
                continue;
            }

            if (objective.type == objectiveType && objective.objectiveID == objectiveID)
            {
                return objective.IsCompleted;
            }
        }

        return false;
    }

    private void ShowToast(string message, ToastType type, string iconKey)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, type, iconKey);
        }
    }
}

using TMPro;
using UnityEngine;

public class QuestHUDUI : MonoBehaviour
{
    [Header("Contenedor")]
    [SerializeField] private GameObject hudRoot;

    [Header("Textos")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text objectiveText;

    [Header("Objetos antiguos que se ocultan")]
    [SerializeField] private GameObject stateTextObject;
    [SerializeField] private GameObject progressTextObject;
    [SerializeField] private GameObject readyToHandInRoot;

    private void Awake()
    {
        HideOldElements();
    }

    private void OnEnable()
    {
        if (QuestController.Instance != null)
        {
            QuestController.Instance.OnQuestUpdated += UpdateQuestHUD;
        }

        UpdateQuestHUD();
    }

    private void OnDisable()
    {
        if (QuestController.Instance != null)
        {
            QuestController.Instance.OnQuestUpdated -= UpdateQuestHUD;
        }
    }

    public void UpdateQuestHUD()
    {
        HideOldElements();

        QuestController questController = QuestController.Instance;

        if (questController == null)
        {
            SetVisible(false);
            return;
        }

        QuestProgress quest = questController.GetFollowedQuest();

        if (quest == null || quest.quest == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        if (titleText != null)
        {
            titleText.text = quest.quest.questName;
        }

        if (objectiveText != null)
        {
            objectiveText.text = GetNextObjectiveText(quest);
        }
    }

    private string GetNextObjectiveText(QuestProgress quest)
    {
        if (quest == null)
        {
            return "";
        }

        if (quest.state == QuestState.Completed)
        {
            if (quest.quest != null && !string.IsNullOrWhiteSpace(quest.quest.handInHint))
            {
                return quest.quest.handInHint;
            }

            return "Misión completada";
        }

        QuestObjective currentObjective = quest.GetCurrentObjective();

        if (currentObjective == null)
        {
            return "";
        }

        return currentObjective.description;
    }

    private void HideOldElements()
    {
        if (stateTextObject != null)
        {
            stateTextObject.SetActive(false);
        }

        if (progressTextObject != null)
        {
            progressTextObject.SetActive(false);
        }

        if (readyToHandInRoot != null)
        {
            readyToHandInRoot.SetActive(false);
        }
    }

    private void SetVisible(bool visible)
    {
        if (hudRoot != null)
        {
            hudRoot.SetActive(visible);
        }
    }
}
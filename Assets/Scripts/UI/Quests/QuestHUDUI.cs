using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestHUDUI : MonoBehaviour
{
    [Header("Contenedor")]
    [SerializeField] private GameObject hudRoot;

    [Header("Textos")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text stateText;

    [Header("Entrega")]
    [SerializeField] private GameObject readyToHandInRoot;
    [SerializeField] private Image npcIconImage;
    [SerializeField] private TMP_Text handInHintText;

    [Header("Colores")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;

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
            titleText.text = "Misión activa:\n" + quest.quest.questName;
        }

        if (quest.state == QuestState.Completed)
        {
            ShowReadyToHandIn(quest, questController);
            return;
        }

        ShowActiveQuest(quest);
    }

    private void ShowActiveQuest(QuestProgress quest)
    {
        QuestObjective currentObjective = quest.GetCurrentObjective();

        if (readyToHandInRoot != null)
        {
            readyToHandInRoot.SetActive(false);
        }

        if (npcIconImage != null)
        {
            npcIconImage.enabled = false;
        }

        if (handInHintText != null)
        {
            handInHintText.text = string.Empty;
        }

        if (stateText != null)
        {
            stateText.text = "En progreso";
            stateText.color = activeColor;
        }

        if (currentObjective == null)
        {
            if (objectiveText != null)
            {
                objectiveText.text = "Sin objetivo asignado";
                objectiveText.color = warningColor;
            }

            if (progressText != null)
            {
                progressText.text = string.Empty;
            }

            return;
        }

        if (objectiveText != null)
        {
            objectiveText.text = currentObjective.description;
            objectiveText.color = currentObjective.IsCompleted ? completedColor : activeColor;
        }

        if (progressText != null)
        {
            progressText.text = currentObjective.currentAmount + " / " + currentObjective.requiredAmount;
            progressText.color = currentObjective.IsCompleted ? completedColor : activeColor;
        }
    }

    private void ShowReadyToHandIn(QuestProgress quest, QuestController questController)
    {
        if (stateText != null)
        {
            stateText.text = "Lista para entregar";
            stateText.color = completedColor;
        }

        if (objectiveText != null)
        {
            objectiveText.text = "✓ Objetivo completado";
            objectiveText.color = completedColor;
        }

        if (progressText != null)
        {
            progressText.text = "Vuelve con el NPC";
            progressText.color = completedColor;
        }

        if (readyToHandInRoot != null)
        {
            readyToHandInRoot.SetActive(true);
        }

        if (handInHintText != null)
        {
            handInHintText.text = questController.GetHandInHint(quest);
        }

        if (npcIconImage != null)
        {
            Sprite icon = questController.GetHandInIcon(quest);
            npcIconImage.sprite = icon;
            npcIconImage.enabled = icon != null;
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

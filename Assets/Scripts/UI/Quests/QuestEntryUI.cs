using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestEntryUI : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text questDescriptionText;
    [SerializeField] private TMP_Text questStateText;

    [Header("Objetivos")]
    [SerializeField] private Transform objectiveList;

    [Header("Botón seguir")]
    [SerializeField] private Button followButton;
    [SerializeField] private TMP_Text followButtonText;

    [Header("Entrega")]
    [SerializeField] private GameObject readyToHandInRoot;
    [SerializeField] private Image npcIconImage;
    [SerializeField] private TMP_Text handInHintText;

    [Header("Formato")]
    [SerializeField] private string followedPrefix = "▶ ";
    [SerializeField] private string activeStateText = "En progreso";
    [SerializeField] private string readyStateText = "Lista para entregar";
    [SerializeField] private string followText = "Seguir";
    [SerializeField] private string followingText = "Siguiendo";

    [Header("Colores opcionales")]
    [SerializeField] private bool overrideTextColors = false;
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color followedTextColor = Color.white;
    [SerializeField] private Color readyTextColor = Color.white;

    private QuestProgress currentQuest;

    public void Setup(QuestProgress questProgress, GameObject objectiveTextPrefab)
    {
        currentQuest = questProgress;

        if (currentQuest == null || currentQuest.quest == null)
        {
            gameObject.SetActive(false);
            return;
        }

        bool isFollowed = QuestController.Instance != null &&
                          QuestController.Instance.IsFollowedQuest(currentQuest.QuestID);

        bool isReadyToHandIn = currentQuest.state == QuestState.Completed;

        SetupTitle(isFollowed, isReadyToHandIn);
        SetupDescription();
        SetupState(isReadyToHandIn);
        SetupFollowButton(isFollowed);
        SetupHandInInfo(isReadyToHandIn);
        SetupObjectives(objectiveTextPrefab);
        ApplyOptionalColors(isFollowed, isReadyToHandIn);
    }

    private void SetupTitle(bool isFollowed, bool isReadyToHandIn)
    {
        if (questNameText == null)
        {
            return;
        }

        string prefix = isFollowed ? followedPrefix : "";
        questNameText.text = prefix + currentQuest.quest.questName;
    }

    private void SetupDescription()
    {
        if (questDescriptionText == null)
        {
            return;
        }

        questDescriptionText.text = currentQuest.quest.description;
        questDescriptionText.gameObject.SetActive(!string.IsNullOrWhiteSpace(currentQuest.quest.description));
    }

    private void SetupState(bool isReadyToHandIn)
    {
        if (questStateText == null)
        {
            return;
        }

        questStateText.text = isReadyToHandIn ? readyStateText : activeStateText;
    }

    private void SetupFollowButton(bool isFollowed)
    {
        if (followButton == null)
        {
            return;
        }

        followButton.onClick.RemoveAllListeners();
        followButton.interactable = !isFollowed;

        if (followButtonText != null)
        {
            followButtonText.text = isFollowed ? followingText : followText;
        }

        followButton.onClick.AddListener(() =>
        {
            if (QuestController.Instance != null)
            {
                QuestController.Instance.FollowQuest(currentQuest.QuestID);
            }
        });
    }

    private void SetupHandInInfo(bool isReadyToHandIn)
    {
        if (readyToHandInRoot != null)
        {
            readyToHandInRoot.SetActive(isReadyToHandIn);
        }

        if (handInHintText != null)
        {
            if (isReadyToHandIn && QuestController.Instance != null)
            {
                handInHintText.text = QuestController.Instance.GetHandInHint(currentQuest);
            }
            else
            {
                handInHintText.text = "";
            }
        }

        if (npcIconImage != null)
        {
            Sprite icon = null;

            if (isReadyToHandIn && QuestController.Instance != null)
            {
                icon = QuestController.Instance.GetHandInIcon(currentQuest);
            }

            npcIconImage.sprite = icon;
            npcIconImage.enabled = icon != null;
        }
    }

    private void SetupObjectives(GameObject objectiveTextPrefab)
    {
        if (objectiveList == null)
        {
            return;
        }

        for (int i = objectiveList.childCount - 1; i >= 0; i--)
        {
            Destroy(objectiveList.GetChild(i).gameObject);
        }

        if (objectiveTextPrefab == null)
        {
            return;
        }

        foreach (QuestObjective objective in currentQuest.objectives)
        {
            GameObject objectiveObject = Instantiate(objectiveTextPrefab, objectiveList);

            QuestObjectiveLineUI objectiveLineUI = objectiveObject.GetComponent<QuestObjectiveLineUI>();

            if (objectiveLineUI != null)
            {
                objectiveLineUI.Setup(objective);
                continue;
            }

            TMP_Text fallbackText = objectiveObject.GetComponent<TMP_Text>();

            if (fallbackText == null)
            {
                fallbackText = objectiveObject.GetComponentInChildren<TMP_Text>();
            }

            if (fallbackText != null)
            {
                string prefix = objective.IsCompleted ? "✓ " : "• ";
                fallbackText.text = prefix + objective.description + " (" + objective.currentAmount + "/" + objective.requiredAmount + ")";
            }
        }
    }

    private void ApplyOptionalColors(bool isFollowed, bool isReadyToHandIn)
    {
        if (!overrideTextColors)
        {
            return;
        }

        Color targetColor = normalTextColor;

        if (isReadyToHandIn)
        {
            targetColor = readyTextColor;
        }
        else if (isFollowed)
        {
            targetColor = followedTextColor;
        }

        SetTextColor(questNameText, targetColor);
        SetTextColor(questDescriptionText, targetColor);
        SetTextColor(questStateText, targetColor);
        SetTextColor(handInHintText, targetColor);
    }

    private void SetTextColor(TMP_Text text, Color color)
    {
        if (text != null)
        {
            text.color = color;
        }
    }
}
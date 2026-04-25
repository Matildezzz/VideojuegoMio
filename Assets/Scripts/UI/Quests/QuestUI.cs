using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [Header("Lista")]
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;

    [Header("Colores")]
    [SerializeField] private Color normalQuestColor = Color.white;
    [SerializeField] private Color followedQuestColor = Color.yellow;
    [SerializeField] private Color completedQuestColor = Color.green;
    [SerializeField] private Color activeObjectiveColor = Color.white;
    [SerializeField] private Color completedObjectiveColor = Color.green;

    private void Start()
    {
        UpdateQuestUI();
    }

    private void OnEnable()
    {
        if (QuestController.Instance != null)
        {
            QuestController.Instance.OnQuestUpdated += UpdateQuestUI;
        }
    }

    private void OnDisable()
    {
        if (QuestController.Instance != null)
        {
            QuestController.Instance.OnQuestUpdated -= UpdateQuestUI;
        }
    }

    public void UpdateQuestUI()
    {
        if (questListContent == null || questEntryPrefab == null || objectiveTextPrefab == null)
        {
            return;
        }

        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        if (QuestController.Instance == null)
        {
            return;
        }

        foreach (QuestProgress quest in QuestController.Instance.activeQuests)
        {
            CreateQuestEntry(quest);
        }
    }

    private void CreateQuestEntry(QuestProgress quest)
    {
        GameObject entry = Instantiate(questEntryPrefab, questListContent);

        Transform nameTransform = entry.transform.Find("QuestNameText");
        Transform objectiveList = entry.transform.Find("ObjectiveList");
        Transform stateTransform = entry.transform.Find("StateText");
        Transform followButtonTransform = entry.transform.Find("FollowButton");
        Transform npcIconTransform = entry.transform.Find("NpcIcon");

        if (nameTransform == null || objectiveList == null)
        {
            Debug.LogError("QuestEntryPrefab missing required children: QuestNameText y ObjectiveList.");
            return;
        }

        TMP_Text questTextName = nameTransform.GetComponent<TMP_Text>();
        TMP_Text stateText = stateTransform != null ? stateTransform.GetComponent<TMP_Text>() : null;
        Button followButton = followButtonTransform != null ? followButtonTransform.GetComponent<Button>() : null;
        Image npcIconImage = npcIconTransform != null ? npcIconTransform.GetComponent<Image>() : null;

        bool isFollowed = QuestController.Instance.IsFollowedQuest(quest.QuestID);
        bool isCompleted = quest.state == QuestState.Completed;

        if (questTextName != null)
        {
            string prefix = isFollowed ? "▶ " : string.Empty;
            questTextName.text = prefix + quest.quest.questName;
            questTextName.color = isCompleted ? completedQuestColor : isFollowed ? followedQuestColor : normalQuestColor;
        }

        if (stateText != null)
        {
            stateText.text = isCompleted ? "Lista para entregar" : "En progreso";
            stateText.color = isCompleted ? completedQuestColor : normalQuestColor;
        }

        if (followButton != null)
        {
            TMP_Text buttonText = followButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = isFollowed ? "Siguiendo" : "Seguir";
            }

            followButton.onClick.RemoveAllListeners();
            followButton.interactable = !isFollowed;
            followButton.onClick.AddListener(() => QuestController.Instance.FollowQuest(quest.QuestID));
        }

        if (npcIconImage != null)
        {
            Sprite icon = QuestController.Instance.GetHandInIcon(quest);
            npcIconImage.sprite = icon;
            npcIconImage.enabled = isCompleted && icon != null;
        }

        foreach (QuestObjective objective in quest.objectives)
        {
            GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
            TMP_Text objText = objTextGO.GetComponent<TMP_Text>();

            if (objText == null)
            {
                continue;
            }

            if (objective.IsCompleted)
            {
                objText.text = "✓ " + objective.description;
                objText.color = completedObjectiveColor;
            }
            else
            {
                objText.text = objective.description + " (" + objective.currentAmount + "/" + objective.requiredAmount + ")";
                objText.color = activeObjectiveColor;
            }
        }
    }
}

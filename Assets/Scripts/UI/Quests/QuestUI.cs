using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform questListContent;
    [SerializeField] private GameObject questEntryPrefab;
    [SerializeField] private GameObject objectiveTextPrefab;

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
        if (questListContent == null || questEntryPrefab == null)
        {
            return;
        }

        ClearList();

        if (QuestController.Instance == null)
        {
            return;
        }

        foreach (QuestProgress questProgress in QuestController.Instance.activeQuests)
        {
            CreateQuestEntry(questProgress);
        }
    }

    private void ClearList()
    {
        for (int i = questListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(questListContent.GetChild(i).gameObject);
        }
    }

    private void CreateQuestEntry(QuestProgress questProgress)
    {
        GameObject entryObject = Instantiate(questEntryPrefab, questListContent);

        QuestEntryUI questEntryUI = entryObject.GetComponent<QuestEntryUI>();

        if (questEntryUI == null)
        {
            Debug.LogError("El prefab de misión necesita el componente QuestEntryUI.");
            return;
        }

        questEntryUI.Setup(questProgress, objectiveTextPrefab);
    }
}
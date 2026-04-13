using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        

        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        // Destroy existing quest entries
        foreach(Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // Build quest entries
        foreach(var quest in QuestController.Instance.activeQuests)
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            
            Transform nameTransform = entry.transform.Find("QuestNameText");
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            if(nameTransform == null || objectiveList == null)
            {
                Debug.LogError("QuestEntryPrefab missing required children");
                return;
            }

            TMP_Text questTextName = nameTransform.GetComponent<TMP_Text>();

            questTextName.text = quest.quest.questName;

            foreach(var objective in quest.objectives)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})"; // Collect 5 Potions (0/5)
            }
        }
    }
}

using TMPro;
using UnityEngine;

public class QuestObjectiveLineUI : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] private TMP_Text objectiveText;

    [Header("Formato")]
    [SerializeField] private string activePrefix = "• ";
    [SerializeField] private string completedPrefix = "✓ ";
    [SerializeField] private bool showProgress = true;

    [Header("Colores opcionales")]
    [SerializeField] private bool overrideTextColors = false;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color completedColor = Color.white;

    private void Awake()
    {
        if (objectiveText == null)
        {
            objectiveText = GetComponent<TMP_Text>();
        }

        if (objectiveText == null)
        {
            objectiveText = GetComponentInChildren<TMP_Text>();
        }
    }

    public void Setup(QuestObjective objective)
    {
        if (objective == null || objectiveText == null)
        {
            return;
        }

        bool completed = objective.IsCompleted;

        string prefix = completed ? completedPrefix : activePrefix;
        string progressText = "";

        if (showProgress)
        {
            progressText = " (" + objective.currentAmount + "/" + objective.requiredAmount + ")";
        }

        objectiveText.text = prefix + objective.description + progressText;

        if (overrideTextColors)
        {
            objectiveText.color = completed ? completedColor : activeColor;
        }
    }
}
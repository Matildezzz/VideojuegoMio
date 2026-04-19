using System.Collections;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private NPCDialogue dialogueData;
    [SerializeField] private NPCFriendship friendship;

    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;

    private enum QuestState
    {
        NotStarted,
        InProgress,
        Completed
    }

    private QuestState questState = QuestState.NotStarted;

    private void Awake()
    {
        if (friendship == null)
        {
            friendship = GetComponent<NPCFriendship>();
        }
    }

    private void Start()
    {
        dialogueUI = DialogueController.Instance;

        if (dialogueUI == null)
        {
            Debug.LogWarning("NPC: no se encontró DialogueController en la escena.");
        }
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null || dialogueUI == null)
        {
            return;
        }

        if (PauseController.IsGamePaused && !isDialogueActive)
        {
            return;
        }

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        if (dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            Debug.LogWarning("NPC: el diálogo no tiene líneas.");
            return;
        }

        SyncQuestState();

        if (questState == QuestState.NotStarted)
        {
            dialogueIndex = 0;
        }
        else if (questState == QuestState.InProgress)
        {
            dialogueIndex = dialogueData.questInProgressIndex;
        }
        else
        {
            dialogueIndex = dialogueData.questCompletedIndex;
        }

        dialogueIndex = Mathf.Clamp(dialogueIndex, 0, dialogueData.dialogueLines.Length - 1);

        isDialogueActive = true;

        if (friendship != null)
        {
            friendship.RegisterTalk();
        }

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        PauseController.SetPause(true);

        DisplayCurrentLine();
    }

    private void SyncQuestState()
    {
        questState = QuestState.NotStarted;

        if (dialogueData == null || dialogueData.quest == null || QuestController.Instance == null)
        {
            return;
        }

        string questID = dialogueData.quest.questID;

        if (QuestController.Instance.IsQuestCompleted(questID) || QuestController.Instance.IsQuestHandedIn(questID))
        {
            questState = QuestState.Completed;
        }
        else if (QuestController.Instance.IsQuestActive(questID))
        {
            questState = QuestState.InProgress;
        }
    }

    private void NextLine()
    {
        if (dialogueData == null || dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            EndDialogue();
            return;
        }

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            return;
        }

        dialogueUI.ClearChoices();

        if (dialogueData.endDialogueLines != null &&
            dialogueIndex < dialogueData.endDialogueLines.Length &&
            dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        if (dialogueData.choices != null)
        {
            foreach (DialogueChoice dialogueChoice in dialogueData.choices)
            {
                if (dialogueChoice != null && dialogueChoice.dialogueIndex == dialogueIndex)
                {
                    DisplayChoice(dialogueChoice);
                    return;
                }
            }
        }

        dialogueIndex++;

        if (dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        string currentLine = dialogueData.dialogueLines[dialogueIndex];

        for (int i = 0; i < currentLine.Length; i++)
        {
            dialogueUI.SetDialogueText(currentLine.Substring(0, i + 1));

            if (dialogueData.voiceSound != null)
            {
                SoundEffectManager.PlayVoice(dialogueData.voiceSound, dialogueData.voicePitch);
            }

            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (dialogueData.autoProgressLines != null &&
            dialogueIndex < dialogueData.autoProgressLines.Length &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    private void DisplayChoice(DialogueChoice choice)
    {
        if (choice.choices == null || choice.nextDialogueIndexes == null || choice.givesQuest == null)
        {
            return;
        }

        int amount = Mathf.Min(choice.choices.Length, choice.nextDialogueIndexes.Length, choice.givesQuest.Length);

        for (int i = 0; i < amount; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            bool givesQuest = choice.givesQuest[i];
            string choiceText = choice.choices[i];

            dialogueUI.CreateChoiceButton(choiceText, () => ChooseOption(nextIndex, givesQuest));
        }
    }

    private void ChooseOption(int nextIndex, bool givesQuest)
    {
        if (givesQuest && dialogueData.quest != null && QuestController.Instance != null)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }

        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        if (questState == QuestState.Completed &&
            dialogueData != null &&
            dialogueData.quest != null &&
            QuestController.Instance != null &&
            !QuestController.Instance.IsQuestHandedIn(dialogueData.quest.questID))
        {
            HandleQuestCompletion(dialogueData.quest);
        }

        StopAllCoroutines();
        isDialogueActive = false;

        if (dialogueUI != null)
        {
            dialogueUI.SetDialogueText("");
            dialogueUI.ClearChoices();
            dialogueUI.ShowDialogueUI(false);
        }

        PauseController.SetPause(false);
    }

    private void HandleQuestCompletion(Quest quest)
    {
        if (quest == null)
        {
            return;
        }

        if (RewardController.Instance != null)
        {
            RewardController.Instance.GiveQuestReward(quest);
        }

        if (QuestController.Instance != null)
        {
            QuestController.Instance.HandInQuest(quest.questID);
        }

        if (friendship != null)
        {
            friendship.RegisterQuestCompleted();
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private NPCDialogue dialogueData;
    [SerializeField] private NPCFriendship friendship;

    [Header("Tienda")]
    [SerializeField] private bool openShopWhenDialogueEnds = false;
    [SerializeField] private ShopNPC shopToOpen;

    [Header("Controles de dialogo")]
    [SerializeField] private bool allowKeyboardAdvance = true;
    [SerializeField] private bool allowEscapeToClose = true;
    [SerializeField] private float inputCooldown = 0.15f;

    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private bool choicesVisible;
    private float nextAllowedDialogueInputTime;

    private enum QuestState
    {
        NotStarted,
        InProgress,
        Completed,
        HandedIn
    }

    private QuestState questState = QuestState.NotStarted;

    public string InteractionText
    {
        get
        {
            string npcName = dialogueData != null && !string.IsNullOrWhiteSpace(dialogueData.npcName)
                ? dialogueData.npcName
                : gameObject.name;

            if (friendship != null)
            {
                return "E - Hablar con " + npcName + "\nG - Regalar objeto";
            }

            return "E - Hablar con " + npcName;
        }
    }

    private void Awake()
    {
        if (friendship == null)
        {
            friendship = GetComponent<NPCFriendship>();
        }

        if (shopToOpen == null)
        {
            shopToOpen = GetComponent<ShopNPC>();
        }
    }

    private void Start()
    {
        dialogueUI = DialogueController.Instance;

        if (dialogueUI == null)
        {
            Debug.LogWarning("NPC: no se encontro DialogueController en la escena.");
        }

        RegisterQuestNpcInfo();
    }

    private void Update()
    {
        if (!isDialogueActive || !allowKeyboardAdvance || Keyboard.current == null)
        {
            return;
        }

        if (Time.unscaledTime < nextAllowedDialogueInputTime)
        {
            return;
        }

        if (allowEscapeToClose && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            BlockDialogueInputBriefly();
            EndDialogue();
            return;
        }

        if (choicesVisible)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            BlockDialogueInputBriefly();
            NextLine();
        }
    }

    private void RegisterQuestNpcInfo()
    {
        if (dialogueData == null || dialogueData.quest == null || QuestController.Instance == null)
        {
            return;
        }

        QuestController.Instance.RegisterQuestNpcInfo(
            dialogueData.quest,
            dialogueData.npcName,
            dialogueData.npcPortrait
        );
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

        if (Time.unscaledTime < nextAllowedDialogueInputTime)
        {
            return;
        }

        BlockDialogueInputBriefly();

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
        RegisterQuestNpcInfo();

        if (dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            Debug.LogWarning("NPC: el dialogo no tiene lineas.");
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
        isTyping = false;
        choicesVisible = false;
        BlockDialogueInputBriefly();

        if (friendship != null)
        {
            friendship.RegisterTalk();
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.NotifyTalkedToNpc();
        }

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.SetFriendship(friendship);
        dialogueUI.ShowDialogueUI(true);
        dialogueUI.RefreshFriendship();
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

        if (QuestController.Instance.IsQuestHandedIn(questID))
        {
            questState = QuestState.HandedIn;
        }
        else if (QuestController.Instance.IsQuestCompleted(questID))
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

        if (dialogueIndex < 0 || dialogueIndex >= dialogueData.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            TryShowChoicesForCurrentLine();
            return;
        }

        if (choicesVisible)
        {
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

        if (TryShowChoicesForCurrentLine())
        {
            return;
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

        if (TryShowChoicesForCurrentLine())
        {
            yield break;
        }

        if (dialogueData.autoProgressLines != null &&
            dialogueIndex < dialogueData.autoProgressLines.Length &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    private bool TryShowChoicesForCurrentLine()
    {
        if (dialogueData == null || dialogueData.choices == null)
        {
            return false;
        }

        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice != null && dialogueChoice.dialogueIndex == dialogueIndex)
            {
                return DisplayChoice(dialogueChoice);
            }
        }

        return false;
    }

    private bool DisplayChoice(DialogueChoice choice)
    {
        dialogueUI.ClearChoices();
        choicesVisible = false;

        if (choice.choices == null || choice.nextDialogueIndexes == null)
        {
            Debug.LogWarning("NPC: hay una eleccion de dialogo mal configurada en " + gameObject.name + ". Faltan choices o nextDialogueIndexes.");
            return false;
        }

        int amount = Mathf.Min(choice.choices.Length, choice.nextDialogueIndexes.Length);

        if (amount <= 0)
        {
            Debug.LogWarning("NPC: hay una eleccion de dialogo vacia en " + gameObject.name + ".");
            return false;
        }

        for (int i = 0; i < amount; i++)
        {
            int capturedNextIndex = choice.nextDialogueIndexes[i];
            bool capturedGivesQuest = choice.givesQuest != null && i < choice.givesQuest.Length && choice.givesQuest[i];
            string choiceText = choice.choices[i];

            GameObject button = dialogueUI.CreateChoiceButton(choiceText, () => ChooseOption(capturedNextIndex, capturedGivesQuest));

            if (button != null)
            {
                choicesVisible = true;
            }
        }

        if (!choicesVisible)
        {
            Debug.LogWarning("NPC: no se pudieron crear botones de dialogo. Revisa Choice Container y Choice Button Prefab en DialogueController.");
        }

        return choicesVisible;
    }

    private void ChooseOption(int nextIndex, bool givesQuest)
    {
        if (givesQuest && dialogueData.quest != null && QuestController.Instance != null)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }

        choicesVisible = false;
        dialogueUI.ClearChoices();
        BlockDialogueInputBriefly();

        if (nextIndex < 0)
        {
            EndDialogue();
            return;
        }

        dialogueIndex = Mathf.Clamp(nextIndex, 0, dialogueData.dialogueLines.Length - 1);
        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (dialogueData == null || dialogueData.dialogueLines == null || dialogueIndex < 0 || dialogueIndex >= dialogueData.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        StopAllCoroutines();
        isTyping = false;
        choicesVisible = false;
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
        isTyping = false;
        choicesVisible = false;
        BlockDialogueInputBriefly();

        if (dialogueUI != null)
        {
            dialogueUI.SetDialogueText("");
            dialogueUI.ClearChoices();
            dialogueUI.ShowDialogueUI(false);
        }

        PauseController.SetPause(false);

        if (openShopWhenDialogueEnds && shopToOpen != null && ShopController.Instance != null)
        {
            ShopController.Instance.OpenShop(shopToOpen);
        }
    }

    private void HandleQuestCompletion(Quest quest)
    {
        if (quest == null || QuestController.Instance == null)
        {
            return;
        }

        bool handedIn = QuestController.Instance.HandInQuest(quest.questID);
        if (!handedIn)
        {
            return;
        }

        if (friendship != null)
        {
            int gainedHalfHearts = friendship.RegisterQuestCompleted();
            dialogueUI?.RefreshFriendship();

            if (gainedHalfHearts > 0 && ToastManager.Instance != null)
            {
                string npcName = dialogueData != null && !string.IsNullOrWhiteSpace(dialogueData.npcName)
                    ? dialogueData.npcName
                    : gameObject.name;

                ToastManager.Instance.ShowToast("+" + FormatHalfHearts(gainedHalfHearts) + " corazón con " + npcName, ToastType.Success, "Heart");
            }
        }
    }

    private string FormatHalfHearts(int halfHearts)
    {
        if (halfHearts % 2 == 0)
        {
            return (halfHearts / 2).ToString();
        }

        return (halfHearts * 0.5f).ToString("0.0");
    }
    private void BlockDialogueInputBriefly()
    {
        nextAllowedDialogueInputTime = Time.unscaledTime + inputCooldown;
    }
}

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

    private enum LocalQuestState
    {
        NotStarted,
        InProgress,
        Completed,
        HandedIn
    }

    private LocalQuestState questState = LocalQuestState.NotStarted;

    public string InteractionText
    {
        get
        {
            string npcName = GetDisplayedNpcName();

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
            GetDisplayedNpcName(),
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

        if (questState == LocalQuestState.NotStarted)
        {
            dialogueIndex = 0;
        }
        else if (questState == LocalQuestState.InProgress)
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

        RegisterTalkObjective();

        dialogueUI.SetNPCInfo(GetDisplayedNpcName(), dialogueData.npcPortrait);
        dialogueUI.SetFriendship(friendship);
        dialogueUI.ShowDialogueUI(true);
        dialogueUI.RefreshFriendship();
        PauseController.SetPause(true);

        DisplayCurrentLine();
    }

    private void RegisterTalkObjective()
    {
        if (QuestController.Instance == null)
        {
            return;
        }

        string npcId = GetNpcId();
        QuestController.Instance.RegisterObjectiveProgress(ObjectiveType.TalkNPC, npcId, 1);

        if (!string.IsNullOrWhiteSpace(dialogueData.npcName))
        {
            QuestController.Instance.RegisterObjectiveProgress(ObjectiveType.TalkNPC, dialogueData.npcName, 1);
        }
    }

    private void SyncQuestState()
    {
        questState = LocalQuestState.NotStarted;

        if (dialogueData == null || dialogueData.quest == null || QuestController.Instance == null)
        {
            return;
        }

        string questID = dialogueData.quest.questID;

        if (QuestController.Instance.IsQuestHandedIn(questID))
        {
            questState = LocalQuestState.HandedIn;
        }
        else if (QuestController.Instance.IsQuestCompleted(questID))
        {
            questState = LocalQuestState.Completed;
        }
        else if (QuestController.Instance.IsQuestActive(questID))
        {
            questState = LocalQuestState.InProgress;
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
            dialogueUI.ShowFullDialogueText(GetCurrentDisplayLine());
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

        string currentLine = GetCurrentDisplayLine();
        int visibleCharacterCount = dialogueUI.PrepareDialogueTextForTyping(currentLine);

        for (int i = 0; i <= visibleCharacterCount; i++)
        {
            dialogueUI.SetVisibleDialogueCharacters(i);

            if (i > 0 && dialogueData.voiceSound != null)
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

    private string GetCurrentDisplayLine()
    {
        if (dialogueData == null || dialogueData.dialogueLines == null || dialogueIndex < 0 || dialogueIndex >= dialogueData.dialogueLines.Length)
        {
            return string.Empty;
        }

        string line = dialogueData.dialogueLines[dialogueIndex];
        return LanguageManager.Instance != null ? LanguageManager.Instance.TranslateDialogueLine(line) : line;
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
            Debug.LogWarning("NPC: hay una eleccion de dialogo mal configurada en " + gameObject.name + ".");
            return false;
        }

        int amount = Mathf.Min(choice.choices.Length, choice.nextDialogueIndexes.Length);

        if (amount <= 0)
        {
            return false;
        }

        for (int i = 0; i < amount; i++)
        {
            int capturedNextIndex = choice.nextDialogueIndexes[i];
            bool capturedGivesQuest = choice.givesQuest != null && i < choice.givesQuest.Length && choice.givesQuest[i];
            string choiceText = choice.choices[i];
            string displayChoiceText = LanguageManager.Instance != null ? LanguageManager.Instance.TranslateDialogueLine(choiceText) : choiceText;

            GameObject button = dialogueUI.CreateChoiceButton(displayChoiceText, () => ChooseOption(capturedNextIndex, capturedGivesQuest));

            if (button != null)
            {
                choicesVisible = true;
            }
        }

        return choicesVisible;
    }

    private void ChooseOption(int nextIndex, bool givesQuest)
    {
        if (givesQuest && dialogueData.quest != null && QuestController.Instance != null)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = LocalQuestState.InProgress;
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

        ApplyLineEffects();
        StopAllCoroutines();
        isTyping = false;
        choicesVisible = false;
        StartCoroutine(TypeLine());
    }

    private void ApplyLineEffects()
    {
        if (dialogueData == null || dialogueData.lineEffects == null)
        {
            return;
        }

        for (int i = 0; i < dialogueData.lineEffects.Length; i++)
        {
            DialogueLineEffect effect = dialogueData.lineEffects[i];

            if (effect == null || effect.dialogueIndex != dialogueIndex)
            {
                continue;
            }

            if (!CanRunLineEffect(effect))
            {
                continue;
            }

            LanguageManager.Instance?.HearWords(effect.wordsToHear);
            LanguageManager.Instance?.GuessWords(effect.wordsToGuess);
            LanguageManager.Instance?.LearnWords(effect.wordsToLearn);
            AlienNameManager.Instance?.LearnItemNames(effect.itemNameIdsToLearn);

            if (AlienNameManager.Instance != null && effect.characterNameIdsToLearn != null)
            {
                for (int j = 0; j < effect.characterNameIdsToLearn.Length; j++)
                {
                    AlienNameManager.Instance.LearnCharacterName(effect.characterNameIdsToLearn[j]);
                }
            }

            if (!HasRequiredItemsForEffect(effect))
            {
                continue;
            }

            if (effect.consumeRequiredItems)
            {
                ConsumeRequiredItemsForEffect(effect);
            }

            if (!string.IsNullOrWhiteSpace(effect.objectiveID) && QuestController.Instance != null)
            {
                QuestController.Instance.RegisterObjectiveProgress(
                    effect.objectiveType,
                    effect.objectiveID,
                    Mathf.Max(1, effect.objectiveAmount)
                );
            }

            GiveLineEffectItems(effect);

            if (AlienDiaryManager.Instance != null && effect.diaryEntriesToUnlock != null)
            {
                for (int j = 0; j < effect.diaryEntriesToUnlock.Length; j++)
                {
                    AlienDiaryEntry entry = effect.diaryEntriesToUnlock[j];

                    if (entry == null)
                    {
                        continue;
                    }

                    AlienDiaryManager.Instance.UnlockEntry(
                        entry.entryId,
                        entry.title,
                        entry.content,
                        entry.category
                    );
                }
            }
        }
    }

    private bool CanRunLineEffect(DialogueLineEffect effect)
    {
        if (effect == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(effect.runOnlyIfDiaryEntryLocked))
        {
            return true;
        }

        if (AlienDiaryManager.Instance == null)
        {
            return true;
        }

        return !AlienDiaryManager.Instance.IsEntryUnlocked(effect.runOnlyIfDiaryEntryLocked);
    }

    private void GiveLineEffectItems(DialogueLineEffect effect)
    {
        if (effect == null || effect.itemsToGive == null || effect.itemsToGive.Length == 0)
        {
            return;
        }

        if (RewardController.Instance == null)
        {
            Debug.LogWarning("NPC: no existe RewardController en la escena. No se pueden dar items.");
            return;
        }

        for (int i = 0; i < effect.itemsToGive.Length; i++)
        {
            DialogueItemGrant grant = effect.itemsToGive[i];

            if (grant == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(grant.itemId))
            {
                Debug.LogWarning("NPC: hay un itemId vacío en un efecto de diálogo.");
                continue;
            }

            int amount = Mathf.Max(1, grant.amount);

            Debug.Log("NPC entrega item: " + grant.itemId + " x" + amount);

            RewardController.Instance.GiveItemReward(grant.itemId, amount);

            if (AlienNameManager.Instance != null)
            {
                AlienNameManager.Instance.ObserveItem(grant.itemId);
            }
        }
    }

    public void EndDialogue()
    {
        if (questState == LocalQuestState.Completed &&
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
                ToastManager.Instance.ShowToast("+" + FormatHalfHearts(gainedHalfHearts) + " corazón con " + GetDisplayedNpcName(), ToastType.Success, "Heart");
            }
        }
    }

    private string GetNpcId()
    {
        if (dialogueData == null)
        {
            return gameObject.name;
        }

        if (!string.IsNullOrWhiteSpace(dialogueData.npcID))
        {
            return dialogueData.npcID;
        }

        if (!string.IsNullOrWhiteSpace(dialogueData.npcName))
        {
            return dialogueData.npcName;
        }

        return gameObject.name;
    }

    private string GetDisplayedNpcName()
    {
        if (dialogueData == null)
        {
            return gameObject.name;
        }

        if (AlienNameManager.Instance == null)
        {
            return string.IsNullOrWhiteSpace(dialogueData.npcName) ? gameObject.name : dialogueData.npcName;
        }

        return AlienNameManager.Instance.GetCharacterDisplayName(GetNpcId(), dialogueData.npcName, dialogueData.alienNpcName);
    }

    public string GetNameForUI()
    {
        return GetDisplayedNpcName();
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

    private bool HasRequiredItemsForEffect(DialogueLineEffect effect)
    {
        if (effect == null || effect.itemsRequired == null || effect.itemsRequired.Length == 0)
        {
            return true;
        }

        if (RewardController.Instance == null)
        {
            Debug.LogWarning("NPC: no existe RewardController en la escena. No se pueden comprobar objetos requeridos.");
            return false;
        }

        bool hasItems = RewardController.Instance.HasRequiredItems(effect.itemsRequired);

        if (!hasItems)
        {
            string message = string.IsNullOrWhiteSpace(effect.missingRequiredItemsMessage)
                ? "No tienes los objetos necesarios."
                : effect.missingRequiredItemsMessage;

            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast(message, ToastType.Warning, "Error");
            }

            Debug.Log("NPC: faltan objetos requeridos para ejecutar el efecto de diálogo.");
        }

        return hasItems;
    }

    private void ConsumeRequiredItemsForEffect(DialogueLineEffect effect)
    {
        if (effect == null || effect.itemsRequired == null || effect.itemsRequired.Length == 0)
        {
            return;
        }

        if (RewardController.Instance == null)
        {
            Debug.LogWarning("NPC: no existe RewardController en la escena. No se pueden consumir objetos requeridos.");
            return;
        }

        bool removed = RewardController.Instance.TakeRequiredItems(effect.itemsRequired);

        if (!removed)
        {
            Debug.LogWarning("NPC: no se pudieron consumir los objetos requeridos.");
        }
    }
}

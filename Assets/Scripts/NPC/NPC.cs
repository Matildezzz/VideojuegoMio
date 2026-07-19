using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private NPCDialogue dialogueData;
    [SerializeField] private NPCFriendship friendship;

    [Header("Tienda")]
    [SerializeField] private bool openShopWhenDialogueEnds;
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
            return friendship != null
                ? "E - Hablar con " + npcName + "\nG - Regalar objeto"
                : "E - Hablar con " + npcName;
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
            return;
        }

        StartDialogue();
    }

    private void StartDialogue()
    {
        RegisterQuestNpcInfo();

        if (dialogueData == null || !dialogueData.HasLines)
        {
            Debug.LogWarning("NPC: el dialogo no tiene lineas.");
            return;
        }

        SyncQuestState();
        dialogueIndex = GetStartingDialogueIndex();

        if (!CanEnterLine(dialogueIndex, true))
        {
            return;
        }

        isDialogueActive = true;
        isTyping = false;
        choicesVisible = false;
        BlockDialogueInputBriefly();

        friendship?.RegisterTalk();
        TutorialManager.Instance?.NotifyTalkedToNpc();
        RegisterTalkObjective();

        dialogueUI.SetNPCInfo(GetDisplayedNpcName(), dialogueData.npcPortrait);
        dialogueUI.SetFriendship(friendship);
        dialogueUI.ShowDialogueUI(true);
        dialogueUI.RefreshFriendship();
        PauseController.SetPause(true);

        DisplayCurrentLine();
    }

    private int GetStartingDialogueIndex()
    {
        if (questState == LocalQuestState.Completed ||
            questState == LocalQuestState.HandedIn)
        {
            return dialogueData.ClampLineIndex(
                dialogueData.questCompletedIndex
            );
        }

        // Si la misión está activa y tenemos los objetos necesarios,
        // comenzamos directamente en la línea de entrega.
        if (questState == LocalQuestState.InProgress)
        {
            int requiredItemsLineIndex = GetReadyRequiredItemsLineIndex();

            if (requiredItemsLineIndex >= 0)
            {
                return requiredItemsLineIndex;
            }
        }

        if (ShouldUseRepeatDialogue())
        {
            return dialogueData.ClampLineIndex(
                dialogueData.repeatDialogueIndex
            );
        }

        if (questState == LocalQuestState.InProgress)
        {
            return dialogueData.ClampLineIndex(
                dialogueData.questInProgressIndex
            );
        }

        return 0;
    }

    private bool ShouldUseRepeatDialogue()
    {
        if (dialogueData == null ||
            dialogueData.repeatDialogueIndex < 0 ||
            dialogueData.lineEffects == null ||
            AlienDiaryManager.Instance == null)
        {
            return false;
        }

        bool foundGuardedEffect = false;

        for (int i = 0; i < dialogueData.lineEffects.Length; i++)
        {
            DialogueLineEffect effect = dialogueData.lineEffects[i];
            if (effect == null || string.IsNullOrWhiteSpace(effect.runOnlyIfDiaryEntryLocked))
            {
                continue;
            }

            foundGuardedEffect = true;

            if (!AlienDiaryManager.Instance.IsEntryUnlocked(effect.runOnlyIfDiaryEntryLocked))
            {
                return false;
            }
        }

        return foundGuardedEffect;
    }

    private void RegisterTalkObjective()
    {
        if (QuestController.Instance == null)
        {
            return;
        }

        string npcId = GetNpcId();
        if (!string.IsNullOrWhiteSpace(npcId))
        {
            QuestController.Instance.RegisterObjectiveProgress(ObjectiveType.TalkNPC, npcId, 1);
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
        if (dialogueData == null || !dialogueData.HasLines || !dialogueData.IsValidLineIndex(dialogueIndex))
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

        if (dialogueData.ShouldEndDialogue(dialogueIndex))
        {
            EndDialogue();
            return;
        }

        if (TryShowChoicesForCurrentLine())
        {
            return;
        }

        TryMoveToLine(dialogueIndex + 1);
    }

    private bool TryMoveToLine(int nextIndex)
    {
        if (!dialogueData.IsValidLineIndex(nextIndex))
        {
            EndDialogue();
            return false;
        }

        if (!CanEnterLine(nextIndex, true))
        {
            EndDialogue();
            return false;
        }

        dialogueIndex = nextIndex;
        DisplayCurrentLine();
        return true;
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

        if (dialogueData.ShouldAutoProgress(dialogueIndex))
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    private string GetCurrentDisplayLine()
    {
        string line = dialogueData != null ? dialogueData.GetLine(dialogueIndex) : string.Empty;
        return TranslateText(line);
    }

    private string TranslateText(string text)
    {
        return LanguageManager.Instance != null
            ? LanguageManager.Instance.TranslateDialogueLine(text)
            : text;
    }

    private bool TryShowChoicesForCurrentLine()
    {
        if (dialogueData == null)
        {
            return false;
        }

        DialogueChoice choice = dialogueData.GetChoiceForLine(dialogueIndex);
        return choice != null && DisplayChoice(choice);
    }

    private bool DisplayChoice(DialogueChoice choice)
    {
        dialogueUI.ClearChoices();
        choicesVisible = false;

        if (choice == null || choice.OptionCount <= 0)
        {
            Debug.LogWarning("NPC: hay una eleccion de dialogo mal configurada en " + gameObject.name + ".");
            return false;
        }

        for (int i = 0; i < choice.OptionCount; i++)
        {
            int capturedNextIndex = choice.nextDialogueIndexes[i];
            bool capturedGivesQuest = choice.GivesQuestAt(i);
            string displayChoiceText = TranslateText(choice.choices[i]);

            GameObject button = dialogueUI.CreateChoiceButton(
                displayChoiceText,
                () => ChooseOption(capturedNextIndex, capturedGivesQuest)
            );

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

        TryMoveToLine(nextIndex);
    }

    private void DisplayCurrentLine()
    {
        if (dialogueData == null || !dialogueData.IsValidLineIndex(dialogueIndex))
        {
            EndDialogue();
            return;
        }

        if (!ApplyLineEffects())
        {
            EndDialogue();
            return;
        }

        StopAllCoroutines();
        isTyping = false;
        choicesVisible = false;
        StartCoroutine(TypeLine());
    }

    private bool CanEnterLine(int lineIndex, bool showFeedback)
    {
        if (dialogueData == null || !dialogueData.IsValidLineIndex(lineIndex))
        {
            return false;
        }

        if (dialogueData.lineEffects == null)
        {
            return true;
        }

        for (int i = 0; i < dialogueData.lineEffects.Length; i++)
        {
            DialogueLineEffect effect = dialogueData.lineEffects[i];

            if (!IsEffectForLine(effect, lineIndex) || !CanRunLineEffect(effect))
            {
                continue;
            }

            if (!HasRequiredItems(effect, showFeedback))
            {
                return false;
            }
        }

        return true;
    }

    private bool ApplyLineEffects()
    {
        if (dialogueData == null || dialogueData.lineEffects == null)
        {
            return true;
        }

        for (int i = 0; i < dialogueData.lineEffects.Length; i++)
        {
            DialogueLineEffect effect = dialogueData.lineEffects[i];

            if (!IsEffectForLine(effect, dialogueIndex) || !CanRunLineEffect(effect))
            {
                continue;
            }

            if (!HasRequiredItems(effect, true))
            {
                return false;
            }

            if (effect.consumeRequiredItems && !ConsumeRequiredItems(effect))
            {
                return false;
            }

            ApplyKnowledgeEffects(effect);
            RegisterLineObjective(effect);
            GiveLineEffectItems(effect);
            UnlockDiaryEntries(effect.diaryEntriesToUnlock);
        }

        return true;
    }

    private bool IsEffectForLine(DialogueLineEffect effect, int lineIndex)
    {
        return effect != null && effect.dialogueIndex == lineIndex;
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

        return AlienDiaryManager.Instance == null ||
               !AlienDiaryManager.Instance.IsEntryUnlocked(effect.runOnlyIfDiaryEntryLocked);
    }

    private void ApplyKnowledgeEffects(DialogueLineEffect effect)
    {
        LanguageManager.Instance?.HearWords(effect.wordsToHear);
        LanguageManager.Instance?.GuessWords(effect.wordsToGuess);
        LanguageManager.Instance?.LearnWords(effect.wordsToLearn);
        AlienNameManager.Instance?.LearnItemNames(effect.itemNameIdsToLearn);

        if (AlienNameManager.Instance == null || effect.characterNameIdsToLearn == null)
        {
            return;
        }

        for (int i = 0; i < effect.characterNameIdsToLearn.Length; i++)
        {
            AlienNameManager.Instance.LearnCharacterName(effect.characterNameIdsToLearn[i]);
        }
    }

    private void RegisterLineObjective(DialogueLineEffect effect)
    {
        if (string.IsNullOrWhiteSpace(effect.objectiveID) || QuestController.Instance == null)
        {
            return;
        }

        QuestController.Instance.RegisterObjectiveProgress(
            effect.objectiveType,
            effect.objectiveID,
            Mathf.Max(1, effect.objectiveAmount)
        );
    }

    private void UnlockDiaryEntries(AlienDiaryEntry[] entries)
    {
        if (AlienDiaryManager.Instance == null || entries == null)
        {
            return;
        }

        for (int i = 0; i < entries.Length; i++)
        {
            AlienDiaryEntry entry = entries[i];
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

            if (grant == null || string.IsNullOrWhiteSpace(grant.itemId))
            {
                Debug.LogWarning("NPC: hay un itemId vacio en un efecto de dialogo.");
                continue;
            }

            int amount = Mathf.Max(1, grant.amount);
            RewardController.Instance.GiveItemReward(grant.itemId, amount);
            AlienNameManager.Instance?.ObserveItem(grant.itemId);
        }
    }

    private bool HasRequiredItems(DialogueLineEffect effect, bool showFeedback)
    {
        if (effect == null || !effect.HasRequiredItems)
        {
            return true;
        }

        if (RewardController.Instance == null)
        {
            Debug.LogWarning("NPC: no existe RewardController en la escena. No se pueden comprobar objetos requeridos.");

            if (showFeedback)
            {
                ShowRequirementFeedback("No se puede acceder al inventario.");
            }

            return false;
        }

        bool hasItems = RewardController.Instance.HasRequiredItems(effect.itemsRequired);

        if (!hasItems && showFeedback)
        {
            ShowRequirementFeedback(effect.GetMissingItemsMessage());
        }

        return hasItems;
    }

    private bool ConsumeRequiredItems(DialogueLineEffect effect)
    {
        if (effect == null || !effect.HasRequiredItems)
        {
            return true;
        }

        if (RewardController.Instance == null)
        {
            Debug.LogWarning("NPC: no existe RewardController en la escena. No se pueden consumir objetos requeridos.");
            return false;
        }

        bool removed = RewardController.Instance.TakeRequiredItems(effect.itemsRequired);

        if (!removed)
        {
            Debug.LogWarning("NPC: no se pudieron consumir los objetos requeridos.");
            ShowRequirementFeedback(effect.GetMissingItemsMessage());
        }

        return removed;
    }

    private void ShowRequirementFeedback(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Warning, "Error");
            return;
        }

        Debug.Log(message);
    }

    public void EndDialogue()
    {
        if (!isDialogueActive)
        {
            return;
        }

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
            dialogueUI.SetDialogueText(string.Empty);
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

        if (friendship == null)
        {
            return;
        }

        int gainedHalfHearts = friendship.RegisterQuestCompleted();
        dialogueUI?.RefreshFriendship();

        if (gainedHalfHearts > 0 && ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(
                "+" + FormatHalfHearts(gainedHalfHearts) + " corazon con " + GetDisplayedNpcName(),
                ToastType.Success,
                "Heart"
            );
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
            return string.IsNullOrWhiteSpace(dialogueData.npcName)
                ? gameObject.name
                : dialogueData.npcName;
        }

        return AlienNameManager.Instance.GetCharacterDisplayName(
            GetNpcId(),
            dialogueData.npcName,
            dialogueData.alienNpcName
        );
    }

    public string GetNameForUI()
    {
        return GetDisplayedNpcName();
    }

    private string FormatHalfHearts(int halfHearts)
    {
        return halfHearts % 2 == 0
            ? (halfHearts / 2).ToString()
            : (halfHearts * 0.5f).ToString("0.0");
    }

    private void BlockDialogueInputBriefly()
    {
        nextAllowedDialogueInputTime = Time.unscaledTime + Mathf.Max(0f, inputCooldown);
    }

    private int GetReadyRequiredItemsLineIndex()
    {
        if (dialogueData == null ||
            dialogueData.lineEffects == null ||
            RewardController.Instance == null)
        {
            return -1;
        }

        for (int i = 0; i < dialogueData.lineEffects.Length; i++)
        {
            DialogueLineEffect effect = dialogueData.lineEffects[i];

            if (effect == null ||
                !effect.HasRequiredItems ||
                !CanRunLineEffect(effect))
            {
                continue;
            }

            if (!dialogueData.IsValidLineIndex(effect.dialogueIndex))
            {
                continue;
            }

            if (RewardController.Instance.HasRequiredItems(
                effect.itemsRequired
            ))
            {
                return effect.dialogueIndex;
            }
        }

        return -1;
    }
}

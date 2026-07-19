using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPCDialogue")]
public class NPCDialogue : ScriptableObject
{
    [Header("Identidad")]
    public string npcID;
    public string npcName;
    public string alienNpcName;
    public Sprite npcPortrait;

    [Header("Dialogo")]
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines;
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    public DialogueChoice[] choices;

    [Header("Efectos narrativos por linea")]
    public DialogueLineEffect[] lineEffects;

    [Header("Mision y repeticion")]
    public int questInProgressIndex;
    public int questCompletedIndex;

    [Tooltip("Linea usada al volver a hablar cuando todos los efectos de una sola vez ya se han ejecutado. Usa -1 para desactivarlo.")]
    public int repeatDialogueIndex = -1;

    public Quest quest;

    public int LineCount => dialogueLines != null ? dialogueLines.Length : 0;
    public bool HasLines => LineCount > 0;

    public bool IsValidLineIndex(int index)
    {
        return index >= 0 && index < LineCount;
    }

    public int ClampLineIndex(int index)
    {
        return HasLines ? Mathf.Clamp(index, 0, LineCount - 1) : 0;
    }

    public string GetLine(int index)
    {
        return IsValidLineIndex(index) ? dialogueLines[index] : string.Empty;
    }

    public bool ShouldAutoProgress(int index)
    {
        return GetFlag(autoProgressLines, index);
    }

    public bool ShouldEndDialogue(int index)
    {
        return GetFlag(endDialogueLines, index);
    }

    public DialogueChoice GetChoiceForLine(int index)
    {
        if (choices == null)
        {
            return null;
        }

        for (int i = 0; i < choices.Length; i++)
        {
            DialogueChoice choice = choices[i];
            if (choice != null && choice.dialogueIndex == index)
            {
                return choice;
            }
        }

        return null;
    }

    private bool GetFlag(bool[] flags, int index)
    {
        return flags != null && index >= 0 && index < flags.Length && flags[index];
    }

    private void OnValidate()
    {
        autoProgressDelay = Mathf.Max(0f, autoProgressDelay);
        typingSpeed = Mathf.Max(0f, typingSpeed);

        int lineCount = LineCount;
        ResizeFlags(ref autoProgressLines, lineCount);
        ResizeFlags(ref endDialogueLines, lineCount);

        if (lineCount == 0)
        {
            questInProgressIndex = 0;
            questCompletedIndex = 0;
            repeatDialogueIndex = -1;
            return;
        }

        questInProgressIndex = Mathf.Clamp(questInProgressIndex, 0, lineCount - 1);
        questCompletedIndex = Mathf.Clamp(questCompletedIndex, 0, lineCount - 1);

        if (repeatDialogueIndex >= 0)
        {
            repeatDialogueIndex = Mathf.Clamp(repeatDialogueIndex, 0, lineCount - 1);
        }

        ValidateChoices();
        ValidateLineEffects();
    }

    private void ResizeFlags(ref bool[] flags, int length)
    {
        if (flags == null)
        {
            flags = new bool[length];
            return;
        }

        if (flags.Length != length)
        {
            Array.Resize(ref flags, length);
        }
    }

    private void ValidateChoices()
    {
        if (choices == null)
        {
            return;
        }

        for (int i = 0; i < choices.Length; i++)
        {
            DialogueChoice choice = choices[i];
            if (choice == null)
            {
                continue;
            }

            if (!IsValidLineIndex(choice.dialogueIndex))
            {
                Debug.LogWarning(name + ": una eleccion apunta a una linea inexistente (" + choice.dialogueIndex + ").", this);
            }

            if (!choice.HasMatchingOptionData)
            {
                Debug.LogWarning(name + ": una eleccion tiene distinto numero de textos e indices de destino.", this);
            }
        }
    }

    private void ValidateLineEffects()
    {
        if (lineEffects == null)
        {
            return;
        }

        for (int i = 0; i < lineEffects.Length; i++)
        {
            DialogueLineEffect effect = lineEffects[i];
            if (effect != null && !IsValidLineIndex(effect.dialogueIndex))
            {
                Debug.LogWarning(name + ": un efecto apunta a una linea inexistente (" + effect.dialogueIndex + ").", this);
            }
        }
    }
}

[Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndexes;
    public bool[] givesQuest;

    public int OptionCount
    {
        get
        {
            if (choices == null || nextDialogueIndexes == null)
            {
                return 0;
            }

            return Mathf.Min(choices.Length, nextDialogueIndexes.Length);
        }
    }

    public bool HasMatchingOptionData =>
        choices != null &&
        nextDialogueIndexes != null &&
        choices.Length == nextDialogueIndexes.Length;

    public bool GivesQuestAt(int optionIndex)
    {
        return givesQuest != null &&
               optionIndex >= 0 &&
               optionIndex < givesQuest.Length &&
               givesQuest[optionIndex];
    }
}

[Serializable]
public class DialogueLineEffect
{
    public int dialogueIndex;

    [Tooltip("Si se rellena, este efecto solo se ejecuta si esa entrada del diario todavia NO esta desbloqueada.")]
    public string runOnlyIfDiaryEntryLocked;

    public string[] wordsToHear;
    public string[] wordsToGuess;
    public string[] wordsToLearn;
    public string[] itemNameIdsToLearn;
    public string[] characterNameIdsToLearn;
    public AlienDiaryEntry[] diaryEntriesToUnlock;

    public ObjectiveType objectiveType = ObjectiveType.Custom;
    public string objectiveID;
    public int objectiveAmount = 1;

    public DialogueItemGrant[] itemsToGive;

    [Header("Objetos requeridos")]
    public DialogueItemGrant[] itemsRequired;
    public bool consumeRequiredItems;
    public string missingRequiredItemsMessage = "No tienes los objetos necesarios.";

    public bool HasRequiredItems => itemsRequired != null && itemsRequired.Length > 0;

    public string GetMissingItemsMessage()
    {
        return string.IsNullOrWhiteSpace(missingRequiredItemsMessage)
            ? "No tienes los objetos necesarios."
            : missingRequiredItemsMessage;
    }
}

[Serializable]
public class DialogueItemGrant
{
    public string itemId;
    public int amount = 1;
}

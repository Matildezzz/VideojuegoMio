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

    [Header("Mision")]
    public int questInProgressIndex;
    public int questCompletedIndex;
    public Quest quest;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndexes;
    public bool[] givesQuest;
}

[System.Serializable]
public class DialogueLineEffect
{
    public int dialogueIndex;
    public string[] wordsToLearn;
    public string[] itemNameIdsToLearn;
    public string[] characterNameIdsToLearn;
    public AlienDiaryEntry[] diaryEntriesToUnlock;
    public ObjectiveType objectiveType = ObjectiveType.Custom;
    public string objectiveID;
    public int objectiveAmount = 1;
}

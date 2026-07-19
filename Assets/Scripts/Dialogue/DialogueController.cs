using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private FriendshipHeartsUI friendshipHeartsUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ShowDialogueUI(false);
    }

    public void ShowDialogueUI(bool show)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(show);
        }

        if (!show && friendshipHeartsUI != null)
        {
            friendshipHeartsUI.Hide();
        }
    }

    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        if (nameText != null)
        {
            nameText.text = npcName;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
        }
    }

    public void SetDialogueText(string text)
    {
        SetDialogueText(text, int.MaxValue);
    }

    public void SetFriendship(NPCFriendship friendship)
    {
        if (friendshipHeartsUI == null)
        {
            return;
        }

        if (friendship == null)
        {
            friendshipHeartsUI.Hide();
            return;
        }

        friendshipHeartsUI.Show(friendship);
    }

    public void RefreshFriendship()
    {
        if (friendshipHeartsUI != null)
        {
            friendshipHeartsUI.Refresh();
        }
    }

    public void ClearChoices()
    {
        if (choiceContainer == null)
        {
            return;
        }

        for (int i = choiceContainer.childCount - 1; i >= 0; i--)
        {
            GameObject child = choiceContainer.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    public GameObject CreateChoiceButton(string choiceText, UnityAction onClick)
    {
        if (choiceContainer == null || choiceButtonPrefab == null)
        {
            return null;
        }

        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);

        TMP_Text buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = choiceText;
        }

        Button button = choiceButton.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("DialogueController: el prefab de eleccion no tiene un componente Button.");
            choiceButton.SetActive(false);
            Destroy(choiceButton);
            return null;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
        return choiceButton;
    }

    public int PrepareDialogueTextForTyping(string text)
    {
        if (dialogueText == null)
        {
            return 0;
        }

        SetDialogueText(text, 0);
        dialogueText.ForceMeshUpdate();
        return dialogueText.textInfo.characterCount;
    }

    public void SetVisibleDialogueCharacters(int amount)
    {
        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = amount;
        }
    }

    public void ShowFullDialogueText(string text)
    {
        SetDialogueText(text);
    }

    private void SetDialogueText(string text, int maxVisibleCharacters)
    {
        if (dialogueText == null)
        {
            return;
        }

        dialogueText.text = text ?? string.Empty;
        dialogueText.maxVisibleCharacters = maxVisibleCharacters;
    }
}

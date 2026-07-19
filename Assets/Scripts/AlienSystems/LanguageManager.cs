using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public enum WordUnderstandingState
{
    Unknown = 0,
    Heard = 1,
    Guessed = 2,
    Learned = 3
}

public sealed class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    [Header("Palabras conocidas al empezar")]
    [SerializeField] private List<string> initialKnownWords = new List<string>();

    [Header("Estados iniciales opcionales")]
    [SerializeField] private List<InitialWordState> initialWordStates = new List<InitialWordState>();

    [Header("Colores por estado")]
    [SerializeField] private Color unknownColor = new Color(0.65f, 0.60f, 0.80f, 1f);
    [SerializeField] private Color heardColor = new Color(1f, 0.55f, 0.20f, 1f);
    [SerializeField] private Color guessedColor = new Color(1f, 0.85f, 0.25f, 1f);
    [SerializeField] private Color learnedColor = new Color(0.45f, 1f, 0.55f, 1f);

    [Header("Visual")]
    [SerializeField] private bool colorizeWords = true;
    [SerializeField] private bool showQuestionMarkOnHeardWords = true;
    [SerializeField] private bool showQuestionMarkOnGuessedWords = true;

    [Header("Feedback")]
    [SerializeField] private bool showToastWhenLearning = true;

    private readonly Dictionary<string, WordUnderstandingState> wordStates = new Dictionary<string, WordUnderstandingState>();

    public event Action<string> OnWordLearned;
    public event Action<string, WordUnderstandingState> OnWordStateChanged;

    private static readonly Regex WordTokenRegex = new Regex(@"\[([^\|\]]+)\|([^\]]+)\]", RegexOptions.Compiled);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadInitialWords();
    }

    private void LoadInitialWords()
    {
        wordStates.Clear();

        for (int i = 0; i < initialKnownWords.Count; i++)
        {
            SetWordStateInternal(initialKnownWords[i], WordUnderstandingState.Learned, false, false);
        }

        for (int i = 0; i < initialWordStates.Count; i++)
        {
            InitialWordState initialState = initialWordStates[i];

            if (initialState == null)
            {
                continue;
            }

            SetWordStateInternal(initialState.word, initialState.state, false, false);
        }
    }

    public bool KnowsWord(string word)
    {
        return GetWordState(word) == WordUnderstandingState.Learned;
    }

    public WordUnderstandingState GetWordState(string word)
    {
        string normalized = Normalize(word);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return WordUnderstandingState.Unknown;
        }

        if (wordStates.TryGetValue(normalized, out WordUnderstandingState state))
        {
            return state;
        }

        return WordUnderstandingState.Unknown;
    }

    public bool HearWord(string word)
    {
        return SetWordState(word, WordUnderstandingState.Heard);
    }

    public bool GuessWord(string word)
    {
        return SetWordState(word, WordUnderstandingState.Guessed);
    }

    public bool LearnWord(string word)
    {
        return SetWordState(word, WordUnderstandingState.Learned);
    }

    public void HearWords(IEnumerable<string> words)
    {
        SetWordsState(words, WordUnderstandingState.Heard);
    }

    public void GuessWords(IEnumerable<string> words)
    {
        SetWordsState(words, WordUnderstandingState.Guessed);
    }

    public void LearnWords(IEnumerable<string> words)
    {
        SetWordsState(words, WordUnderstandingState.Learned);
    }

    private void SetWordsState(IEnumerable<string> words, WordUnderstandingState state)
    {
        if (words == null)
        {
            return;
        }

        foreach (string word in words)
        {
            SetWordState(word, state);
        }
    }

    public bool SetWordState(string word, WordUnderstandingState targetState)
    {
        return SetWordStateInternal(word, targetState, true, true);
    }

    private bool SetWordStateInternal(string word, WordUnderstandingState targetState, bool invokeEvents, bool showToast)
    {
        string normalized = Normalize(word);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        WordUnderstandingState currentState = GetWordState(normalized);

        if ((int)currentState >= (int)targetState)
        {
            return false;
        }

        wordStates[normalized] = targetState;

        if (invokeEvents)
        {
            OnWordStateChanged?.Invoke(normalized, targetState);
        }

        if (targetState == WordUnderstandingState.Learned)
        {
            AlienDiaryManager.Instance?.RegisterLearnedWord(normalized);

            if (invokeEvents)
            {
                OnWordLearned?.Invoke(normalized);
            }

            if (showToast && showToastWhenLearning && ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("Nueva palabra aprendida: " + normalized.ToUpper(), ToastType.Success, "QuestComplete");
            }
        }

        return true;
    }

    public string TranslateDialogueLine(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return line;
        }

        return WordTokenRegex.Replace(line, match =>
        {
            string humanWord = match.Groups[1].Value.Trim();
            string alienWord = match.Groups[2].Value.Trim();

            WordUnderstandingState state = GetWordState(humanWord);

            switch (state)
            {
                case WordUnderstandingState.Heard:
                    return Colorize(alienWord + (showQuestionMarkOnHeardWords ? "?" : ""), heardColor);

                case WordUnderstandingState.Guessed:
                    return Colorize(humanWord + (showQuestionMarkOnGuessedWords ? "?" : ""), guessedColor);

                case WordUnderstandingState.Learned:
                    return Colorize(humanWord, learnedColor);

                default:
                    return Colorize(alienWord, unknownColor);
            }
        });
    }

    private string Colorize(string text, Color color)
    {
        if (!colorizeWords)
        {
            return text;
        }

        string hex = ColorUtility.ToHtmlStringRGBA(color);
        return "<color=#" + hex + ">" + text + "</color>";
    }

    public LanguageSaveData CaptureSaveData()
    {
        LanguageSaveData saveData = new LanguageSaveData();

        foreach (KeyValuePair<string, WordUnderstandingState> pair in wordStates)
        {
            saveData.wordStates.Add(new WordStateSaveData
            {
                word = pair.Key,
                state = pair.Value
            });

            if (pair.Value == WordUnderstandingState.Learned)
            {
                saveData.learnedWords.Add(pair.Key);
            }
        }

        return saveData;
    }

    public void RestoreFromSaveData(LanguageSaveData saveData)
    {
        LoadInitialWords();

        if (saveData == null)
        {
            return;
        }

        if (saveData.learnedWords != null)
        {
            for (int i = 0; i < saveData.learnedWords.Count; i++)
            {
                SetWordStateInternal(saveData.learnedWords[i], WordUnderstandingState.Learned, false, false);
            }
        }

        if (saveData.wordStates != null)
        {
            for (int i = 0; i < saveData.wordStates.Count; i++)
            {
                WordStateSaveData wordState = saveData.wordStates[i];

                if (wordState == null)
                {
                    continue;
                }

                SetWordStateInternal(wordState.word, wordState.state, false, false);
            }
        }
    }

    private string Normalize(string word)
    {
        return string.IsNullOrWhiteSpace(word) ? string.Empty : word.Trim().ToLowerInvariant();
    }
}

[Serializable]
public class InitialWordState
{
    public string word;
    public WordUnderstandingState state = WordUnderstandingState.Learned;
}

[Serializable]
public class LanguageSaveData
{
    public List<string> learnedWords = new List<string>();
    public List<WordStateSaveData> wordStates = new List<WordStateSaveData>();
}

[Serializable]
public class WordStateSaveData
{
    public string word;
    public WordUnderstandingState state;
}
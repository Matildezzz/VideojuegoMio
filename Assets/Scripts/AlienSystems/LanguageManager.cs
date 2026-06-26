using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public sealed class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    [Header("Palabras conocidas al empezar")]
    [SerializeField] private List<string> initialKnownWords = new List<string>();

    [Header("Feedback")]
    [SerializeField] private bool showToastWhenLearning = true;

    private readonly HashSet<string> learnedWords = new HashSet<string>();

    public event Action<string> OnWordLearned;

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
        learnedWords.Clear();

        for (int i = 0; i < initialKnownWords.Count; i++)
        {
            string word = Normalize(initialKnownWords[i]);
            if (!string.IsNullOrWhiteSpace(word))
            {
                learnedWords.Add(word);
            }
        }
    }

    public bool KnowsWord(string word)
    {
        return learnedWords.Contains(Normalize(word));
    }

    public bool LearnWord(string word)
    {
        string normalized = Normalize(word);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        bool added = learnedWords.Add(normalized);
        if (!added)
        {
            return false;
        }

        AlienDiaryManager.Instance?.RegisterLearnedWord(normalized);
        OnWordLearned?.Invoke(normalized);

        if (showToastWhenLearning && ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("Nueva palabra aprendida: " + normalized.ToUpper(), ToastType.Success, "QuestComplete");
        }

        return true;
    }

    public void LearnWords(IEnumerable<string> words)
    {
        if (words == null)
        {
            return;
        }

        foreach (string word in words)
        {
            LearnWord(word);
        }
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
            return KnowsWord(humanWord) ? humanWord : alienWord;
        });
    }

    public LanguageSaveData CaptureSaveData()
    {
        LanguageSaveData saveData = new LanguageSaveData();
        saveData.learnedWords.AddRange(learnedWords);
        return saveData;
    }

    public void RestoreFromSaveData(LanguageSaveData saveData)
    {
        LoadInitialWords();

        if (saveData == null || saveData.learnedWords == null)
        {
            return;
        }

        for (int i = 0; i < saveData.learnedWords.Count; i++)
        {
            string word = Normalize(saveData.learnedWords[i]);
            if (!string.IsNullOrWhiteSpace(word))
            {
                learnedWords.Add(word);
            }
        }
    }

    private string Normalize(string word)
    {
        return string.IsNullOrWhiteSpace(word) ? string.Empty : word.Trim().ToLowerInvariant();
    }
}

[Serializable]
public class LanguageSaveData
{
    public List<string> learnedWords = new List<string>();
}

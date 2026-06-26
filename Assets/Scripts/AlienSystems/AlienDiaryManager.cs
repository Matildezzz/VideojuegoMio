using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AlienDiaryManager : MonoBehaviour
{
    public static AlienDiaryManager Instance { get; private set; }

    [Header("Entradas iniciales")]
    [SerializeField] private List<AlienDiaryEntry> initialEntries = new List<AlienDiaryEntry>();

    [Header("Feedback")]
    [SerializeField] private bool showToastWhenUnlockingEntry = true;

    private readonly Dictionary<string, AlienDiaryEntry> entriesById = new Dictionary<string, AlienDiaryEntry>();
    private readonly HashSet<string> learnedWords = new HashSet<string>();

    public event Action OnDiaryChanged;

    public IReadOnlyCollection<AlienDiaryEntry> Entries => entriesById.Values;
    public IReadOnlyCollection<string> LearnedWords => learnedWords;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadInitialEntries();
    }

    private void LoadInitialEntries()
    {
        entriesById.Clear();

        for (int i = 0; i < initialEntries.Count; i++)
        {
            AlienDiaryEntry source = initialEntries[i];
            if (source == null || string.IsNullOrWhiteSpace(source.entryId))
            {
                continue;
            }

            entriesById[source.entryId] = source.Clone();
        }
    }

    public bool UnlockEntry(string entryId, string title, string content, string category = "Observaciones")
    {
        if (string.IsNullOrWhiteSpace(entryId))
        {
            return false;
        }

        AlienDiaryEntry entry;
        if (!entriesById.TryGetValue(entryId, out entry))
        {
            entry = new AlienDiaryEntry();
            entry.entryId = entryId;
            entriesById.Add(entryId, entry);
        }

        bool wasUnlocked = entry.unlocked;
        entry.title = string.IsNullOrWhiteSpace(title) ? entry.title : title;
        entry.content = string.IsNullOrWhiteSpace(content) ? entry.content : content;
        entry.category = string.IsNullOrWhiteSpace(category) ? entry.category : category;
        entry.unlocked = true;

        OnDiaryChanged?.Invoke();

        if (!wasUnlocked && showToastWhenUnlockingEntry && ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("Nueva observación: " + entry.title, ToastType.Success, "QuestComplete");
        }

        return !wasUnlocked;
    }

    public void RegisterLearnedWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return;
        }

        string normalized = word.Trim().ToLowerInvariant();
        bool added = learnedWords.Add(normalized);

        if (added)
        {
            UnlockEntry(
                "word_" + normalized,
                "Palabra aprendida: " + normalized.ToUpper(),
                "El sonido humano '" + normalized + "' parece tener un significado estable. Seguir observando usos.",
                "Palabras aprendidas");
        }
    }

    public AlienDiarySaveData CaptureSaveData()
    {
        AlienDiarySaveData saveData = new AlienDiarySaveData();

        foreach (AlienDiaryEntry entry in entriesById.Values)
        {
            if (entry != null)
            {
                saveData.entries.Add(entry.Clone());
            }
        }

        saveData.learnedWords.AddRange(learnedWords);
        return saveData;
    }

    public void RestoreFromSaveData(AlienDiarySaveData saveData)
    {
        LoadInitialEntries();
        learnedWords.Clear();

        if (saveData == null)
        {
            OnDiaryChanged?.Invoke();
            return;
        }

        if (saveData.entries != null)
        {
            for (int i = 0; i < saveData.entries.Count; i++)
            {
                AlienDiaryEntry entry = saveData.entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.entryId))
                {
                    continue;
                }

                entriesById[entry.entryId] = entry.Clone();
            }
        }

        if (saveData.learnedWords != null)
        {
            for (int i = 0; i < saveData.learnedWords.Count; i++)
            {
                string word = saveData.learnedWords[i];
                if (!string.IsNullOrWhiteSpace(word))
                {
                    learnedWords.Add(word.Trim().ToLowerInvariant());
                }
            }
        }

        OnDiaryChanged?.Invoke();
    }
}

[Serializable]
public class AlienDiaryEntry
{
    public string entryId;
    public string title;
    [TextArea(2, 6)] public string content;
    public string category = "Observaciones";
    public bool unlocked;

    public AlienDiaryEntry Clone()
    {
        return new AlienDiaryEntry
        {
            entryId = entryId,
            title = title,
            content = content,
            category = category,
            unlocked = unlocked
        };
    }
}

[Serializable]
public class AlienDiarySaveData
{
    public List<AlienDiaryEntry> entries = new List<AlienDiaryEntry>();
    public List<string> learnedWords = new List<string>();
}

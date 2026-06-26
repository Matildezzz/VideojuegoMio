using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AlienNameManager : MonoBehaviour
{
    public static AlienNameManager Instance { get; private set; }

    [Header("Configuracion")]
    [SerializeField] private bool humanItemNamesKnownByDefault;
    [SerializeField] private bool humanCharacterNamesKnownByDefault;

    private readonly HashSet<string> observedItemIds = new HashSet<string>();
    private readonly HashSet<string> learnedItemNameIds = new HashSet<string>();
    private readonly HashSet<string> learnedCharacterNameIds = new HashSet<string>();

    public event Action OnNamesChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static string GetDisplayName(ItemData item)
    {
        if (Instance == null)
        {
            return item != null ? item.DisplayName : "Objeto";
        }

        return Instance.GetItemDisplayName(item);
    }

    public static string GetDescription(ItemData item, string fallback)
    {
        if (Instance == null)
        {
            return string.IsNullOrWhiteSpace(fallback) ? string.Empty : fallback;
        }

        return Instance.GetItemDescription(item, fallback);
    }

    public string GetItemDisplayName(ItemData item)
    {
        if (item == null)
        {
            return "Objeto";
        }

        if (humanItemNamesKnownByDefault || IsItemNameLearned(item.ItemId))
        {
            return string.IsNullOrWhiteSpace(item.DisplayName) ? "Objeto" : item.DisplayName;
        }

        return string.IsNullOrWhiteSpace(item.AlienDisplayName) ? item.DisplayName : item.AlienDisplayName;
    }

    public string GetItemDescription(ItemData item, string fallback)
    {
        if (item == null)
        {
            return fallback;
        }

        if (humanItemNamesKnownByDefault || IsItemNameLearned(item.ItemId))
        {
            return string.IsNullOrWhiteSpace(item.Description) ? fallback : item.Description;
        }

        return string.IsNullOrWhiteSpace(item.AlienDescription) ? fallback : item.AlienDescription;
    }

    public string GetCharacterDisplayName(string characterId, string humanName, string alienName)
    {
        if (humanCharacterNamesKnownByDefault || IsCharacterNameLearned(characterId))
        {
            return string.IsNullOrWhiteSpace(humanName) ? "Humano" : humanName;
        }

        return string.IsNullOrWhiteSpace(alienName) ? humanName : alienName;
    }

    public void ObserveItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return;
        }

        if (observedItemIds.Add(itemId))
        {
            OnNamesChanged?.Invoke();
        }
    }

    public bool LearnItemName(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        bool added = learnedItemNameIds.Add(itemId);
        if (!added)
        {
            return false;
        }

        OnNamesChanged?.Invoke();
        return true;
    }

    public void LearnItemNames(IEnumerable<string> itemIds)
    {
        if (itemIds == null)
        {
            return;
        }

        foreach (string itemId in itemIds)
        {
            LearnItemName(itemId);
        }
    }

    public bool LearnCharacterName(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return false;
        }

        bool added = learnedCharacterNameIds.Add(characterId);
        if (!added)
        {
            return false;
        }

        OnNamesChanged?.Invoke();
        return true;
    }

    public bool IsItemNameLearned(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId) && learnedItemNameIds.Contains(itemId);
    }

    public bool IsCharacterNameLearned(string characterId)
    {
        return !string.IsNullOrWhiteSpace(characterId) && learnedCharacterNameIds.Contains(characterId);
    }

    public AlienNameSaveData CaptureSaveData()
    {
        AlienNameSaveData saveData = new AlienNameSaveData();
        saveData.observedItemIds.AddRange(observedItemIds);
        saveData.learnedItemNameIds.AddRange(learnedItemNameIds);
        saveData.learnedCharacterNameIds.AddRange(learnedCharacterNameIds);
        return saveData;
    }

    public void RestoreFromSaveData(AlienNameSaveData saveData)
    {
        observedItemIds.Clear();
        learnedItemNameIds.Clear();
        learnedCharacterNameIds.Clear();

        if (saveData == null)
        {
            OnNamesChanged?.Invoke();
            return;
        }

        AddRange(observedItemIds, saveData.observedItemIds);
        AddRange(learnedItemNameIds, saveData.learnedItemNameIds);
        AddRange(learnedCharacterNameIds, saveData.learnedCharacterNameIds);
        OnNamesChanged?.Invoke();
    }

    private void AddRange(HashSet<string> target, List<string> source)
    {
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(source[i]))
            {
                target.Add(source[i]);
            }
        }
    }
}

[Serializable]
public class AlienNameSaveData
{
    public List<string> observedItemIds = new List<string>();
    public List<string> learnedItemNameIds = new List<string>();
    public List<string> learnedCharacterNameIds = new List<string>();
}

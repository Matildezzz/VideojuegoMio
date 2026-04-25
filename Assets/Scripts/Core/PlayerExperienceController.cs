using System;
using UnityEngine;

public class PlayerExperienceController : MonoBehaviour
{
    public static PlayerExperienceController Instance { get; private set; }

    [Header("Experiencia")]
    [SerializeField] private int currentExperience = 0;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int experienceToNextLevel = 100;
    [SerializeField] private float levelRequirementMultiplier = 1.25f;

    public int CurrentExperience => currentExperience;
    public int CurrentLevel => currentLevel;
    public int ExperienceToNextLevel => experienceToNextLevel;

    public event Action<int> OnExperienceChanged;
    public event Action<int> OnLevelChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentLevel = Mathf.Max(1, currentLevel);
        experienceToNextLevel = Mathf.Max(1, experienceToNextLevel);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentExperience += amount;

        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            currentLevel++;
            experienceToNextLevel = Mathf.Max(1, Mathf.RoundToInt(experienceToNextLevel * levelRequirementMultiplier));
            OnLevelChanged?.Invoke(currentLevel);

            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("Nivel " + currentLevel + " alcanzado", ToastType.Success, "QuestComplete");
            }
        }

        OnExperienceChanged?.Invoke(currentExperience);
    }

    public void SetExperience(int experience, int level, int nextLevelRequirement)
    {
        currentExperience = Mathf.Max(0, experience);
        currentLevel = Mathf.Max(1, level);
        experienceToNextLevel = Mathf.Max(1, nextLevelRequirement);

        OnExperienceChanged?.Invoke(currentExperience);
        OnLevelChanged?.Invoke(currentLevel);
    }
}

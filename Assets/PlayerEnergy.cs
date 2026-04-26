using System;
using UnityEngine;

public sealed class PlayerEnergy : MonoBehaviour
{
    public event Action<int, int> OnEnergyChanged;

    [Header("Energia")]
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private int currentEnergy = 100;

    public int CurrentEnergy => currentEnergy;
    public int MaxEnergy => maxEnergy;
    public bool IsFull => currentEnergy >= maxEnergy;

    private void Awake()
    {
        if (maxEnergy < 1)
        {
            maxEnergy = 1;
        }

        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        NotifyEnergyChanged();
    }

    public bool CanSpendEnergy(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        return currentEnergy >= amount;
    }

    public bool SpendEnergy(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (currentEnergy < amount)
        {
            ShowNoEnergyToast();
            return false;
        }

        currentEnergy = Mathf.Max(0, currentEnergy - amount);
        NotifyEnergyChanged();
        return true;
    }

    public void RestoreEnergy(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int previousEnergy = currentEnergy;
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);

        if (currentEnergy != previousEnergy)
        {
            NotifyEnergyChanged();
        }
    }

    public void RestoreFull()
    {
        currentEnergy = maxEnergy;
        NotifyEnergyChanged();
    }

    public void SetEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(amount, 0, maxEnergy);
        NotifyEnergyChanged();
    }

    private void NotifyEnergyChanged()
    {
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    private void ShowNoEnergyToast()
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("No tienes energia suficiente.", ToastType.Warning, "Error");
        }
        else
        {
            Debug.Log("No tienes energia suficiente.");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxEnergy < 1)
        {
            maxEnergy = 1;
        }

        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
    }
#endif
}
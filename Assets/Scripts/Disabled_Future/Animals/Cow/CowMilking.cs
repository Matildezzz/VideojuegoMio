using UnityEngine;
using UnityEngine.InputSystem;

public class CowMilking : MonoBehaviour
{
    [Header("Milk")]
    [SerializeField] private ItemData milkItemData;
    [SerializeField] private int milkAmount = 1;

    [Header("State")]
    [SerializeField] private bool canBeMilked = true;
    [SerializeField] private int lastMilkingDay = -1;

    private IItemReceiver currentReceiver;
    private bool playerInside;

    private void OnEnable()
    {
        TimeManager.OnNewDay += HandleNewDay;
    }

    private void OnDisable()
    {
        TimeManager.OnNewDay -= HandleNewDay;
    }

    private void Start()
    {
        RefreshMilkState();
    }

    private void Update()
    {
        if (!playerInside || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryMilkCow();
        }
    }

    private int GetCurrentDay()
    {
        return TimeManager.Instance != null ? TimeManager.Instance.Day : TimeManager.CurrentDay;
    }

    private void HandleNewDay(int currentDay)
    {
        canBeMilked = true;
    }

    private void RefreshMilkState()
    {
        if (lastMilkingDay != GetCurrentDay())
        {
            canBeMilked = true;
        }
    }

    private void TryMilkCow()
    {
        if (!canBeMilked)
        {
            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("La vaca ya no tiene leche hoy", ToastType.Warning, "Error");
            }

            return;
        }

        if (currentReceiver == null)
        {
            return;
        }

        if (milkItemData == null)
        {
            Debug.LogWarning("Falta asignar el ItemData de la leche en la vaca.");
            return;
        }

        bool added = currentReceiver.TryAddItem(milkItemData, milkAmount);

        if (!added)
        {
            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("Inventario lleno", ToastType.Error, "Error");
            }

            return;
        }

        lastMilkingDay = GetCurrentDay();
        canBeMilked = false;
        AlienNameManager.Instance?.ObserveItem(milkItemData.ItemId);

        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("+" + milkAmount + " " + AlienNameManager.GetDisplayName(milkItemData), ToastType.Success, "Pickup");
        }
    }

    public void SetPlayerInside(IItemReceiver receiver)
    {
        currentReceiver = receiver;
        playerInside = true;
    }

    public void ClearPlayerInside(IItemReceiver receiver)
    {
        if (receiver != currentReceiver)
        {
            return;
        }

        currentReceiver = null;
        playerInside = false;
    }
}

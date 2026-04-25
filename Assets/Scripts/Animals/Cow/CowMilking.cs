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
        DayManager.OnNewDay += HandleNewDay;
    }

    private void OnDisable()
    {
        DayManager.OnNewDay -= HandleNewDay;
    }

    private void Start()
    {
        RefreshMilkState();
    }

    private void Update()
    {
        if (!playerInside)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryMilkCow();
        }
    }

    private void HandleNewDay(int currentDay)
    {
        canBeMilked = true;
        Debug.Log("La vaca vuelve a tener leche.");
    }

    private void RefreshMilkState()
    {
        if (lastMilkingDay != DayManager.CurrentDay)
        {
            canBeMilked = true;
        }
    }

    private void TryMilkCow()
    {
        if (!canBeMilked)
        {
            Debug.Log("La vaca ya ha sido ordeñada hoy.");

            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("La vaca ya no tiene leche hoy", ToastType.Warning, "Error");
            }

            return;
        }

        if (currentReceiver == null)
        {
            Debug.LogWarning("No se ha encontrado IItemReceiver en el jugador.");
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
            Debug.Log("Inventario lleno. No puedes recoger la leche.");

            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("Inventario lleno", ToastType.Error, "Error");
            }

            return;
        }

        lastMilkingDay = DayManager.CurrentDay;
        canBeMilked = false;

        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("+" + milkAmount + " " + milkItemData.DisplayName, ToastType.Success, "Pickup");
        }

        Debug.Log("Has ordeñado la vaca y has conseguido leche.");
    }

    public void SetPlayerInside(IItemReceiver receiver)
    {
        currentReceiver = receiver;
        playerInside = true;

        Debug.Log("Jugador cerca de la vaca. Pulsa E para ordeñar.");
    }

    public void ClearPlayerInside(IItemReceiver receiver)
    {
        if (receiver != currentReceiver)
        {
            return;
        }

        currentReceiver = null;
        playerInside = false;

        Debug.Log("Jugador se alejó de la vaca.");
    }
}
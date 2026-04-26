using UnityEngine;

public sealed class PlayerItemActions : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerToolAnimator toolAnimator;

    private ISeedUser seedUser;
    private IToolUser toolUser;
    private IPlaceableUser placeableUser;
    private PlayerHealth playerHealth;
    private PlayerEnergy playerEnergy;

    private void Awake()
    {
        seedUser = GetComponent<ISeedUser>();
        toolUser = GetComponent<IToolUser>();
        placeableUser = GetComponent<IPlaceableUser>();
        playerHealth = GetComponent<PlayerHealth>();
        playerEnergy = GetComponent<PlayerEnergy>();

        if (toolAnimator == null)
        {
            toolAnimator = GetComponent<PlayerToolAnimator>();
        }
    }

    public bool TryUse(ItemData item, GameObject user)
    {
        if (item == null)
        {
            ShowError("Selecciona un objeto valido.");
            return false;
        }

        if (item is ConsumableItemData consumable)
        {
            return TryUseConsumable(consumable);
        }

        if (item is SeedItemData seed)
        {
            return TryUseSeed(seed);
        }

        if (item is ToolItemData tool)
        {
            return TryUseTool(tool);
        }

        if (item is PlaceableItemData placeable)
        {
            return TryUsePlaceable(placeable);
        }

        ShowWarning("Este objeto no se puede usar directamente.");
        return false;
    }

    private bool TryUseConsumable(ConsumableItemData consumable)
    {
        bool changedSomething = false;

        if (playerHealth != null && consumable.HealthRestore > 0 && playerHealth.CurrentHealth < playerHealth.MaxHealth)
        {
            playerHealth.Heal(consumable.HealthRestore);
            changedSomething = true;
        }

        if (playerEnergy != null && consumable.EnergyRestore > 0 && !playerEnergy.IsFull)
        {
            playerEnergy.RestoreEnergy(consumable.EnergyRestore);
            changedSomething = true;
        }

        if (!changedSomething)
        {
            ShowWarning("No necesitas usar este consumible ahora.");
        }

        return changedSomething;
    }

    private bool TryUseSeed(SeedItemData seed)
    {
        if (seedUser == null)
        {
            ShowError("No puedes plantar semillas con este personaje.");
            return false;
        }

        return seedUser.TryUseSeed(seed);
    }

    private bool TryUseTool(ToolItemData tool)
    {
        if (toolUser == null)
        {
            ShowError("No puedes usar herramientas con este personaje.");
            return false;
        }

        if (playerEnergy != null && !playerEnergy.CanSpendEnergy(tool.EnergyCost))
        {
            playerEnergy.SpendEnergy(tool.EnergyCost);
            return false;
        }

        bool used = toolUser.TryUseTool(tool);

        if (!used)
        {
            return false;
        }

        if (playerEnergy != null)
        {
            playerEnergy.SpendEnergy(tool.EnergyCost);
        }

        if (toolAnimator != null)
        {
            toolAnimator.PlayToolAnimation(tool);
        }

        return true;
    }

    private bool TryUsePlaceable(PlaceableItemData placeable)
    {
        if (placeableUser == null)
        {
            ShowError("No puedes colocar objetos aqui.");
            return false;
        }

        return placeableUser.TryPlace(placeable);
    }

    private void ShowWarning(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Warning, "Error");
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void ShowError(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Error, "Error");
        }
        else
        {
            Debug.Log(message);
        }
    }
}
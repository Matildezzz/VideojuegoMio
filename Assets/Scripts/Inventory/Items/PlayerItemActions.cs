using UnityEngine;

public sealed class PlayerItemActions : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerToolAnimator toolAnimator;

    private ISeedUser seedUser;
    private IToolUser toolUser;
    private IPlaceableUser placeableUser;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        seedUser = GetComponent<ISeedUser>();
        toolUser = GetComponent<IToolUser>();
        placeableUser = GetComponent<IPlaceableUser>();
        playerHealth = GetComponent<PlayerHealth>();

        if (toolAnimator == null)
        {
            toolAnimator = GetComponent<PlayerToolAnimator>();
        }
    }

    public bool TryUse(ItemData item, GameObject user)
    {
        if (item == null)
        {
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

        return false;
    }

    private bool TryUseConsumable(ConsumableItemData consumable)
    {
        bool changedSomething = false;

        if (playerHealth != null && consumable.HealthRestore > 0)
        {
            playerHealth.Heal(consumable.HealthRestore);
            changedSomething = true;
        }

        return changedSomething;
    }

    private bool TryUseSeed(SeedItemData seed)
    {
        if (seedUser == null)
        {
            return false;
        }

        return seedUser.TryUseSeed(seed);
    }

    private bool TryUseTool(ToolItemData tool)
    {
        if (toolUser == null)
        {
            return false;
        }

        bool used = toolUser.TryUseTool(tool);

        if (used && toolAnimator != null)
        {
            toolAnimator.PlayToolAnimation(tool);
        }

        return used;
    }

    private bool TryUsePlaceable(PlaceableItemData placeable)
    {
        if (placeableUser == null)
        {
            return false;
        }

        return placeableUser.TryPlace(placeable);
    }
}
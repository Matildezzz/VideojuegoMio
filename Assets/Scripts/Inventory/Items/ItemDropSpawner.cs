using UnityEngine;

public sealed class ItemDropSpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private float dropDistance = 1.2f;
    [SerializeField] private float pickupBlockDuration = 0.2f;

    public DroppedItem Spawn(ItemData item, int amount, Vector3 origin, Vector2 direction)
    {
        if (item == null)
        {
            Debug.LogWarning("ItemDropSpawner: item es null.");
            return null;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("ItemDropSpawner: amount debe ser mayor que 0.");
            return null;
        }

        if (item.WorldPrefab == null)
        {
            Debug.LogWarning("ItemDropSpawner: el item no tiene WorldPrefab -> " + item.DisplayName);
            return null;
        }

        Vector2 finalDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
        Vector3 spawnPosition = origin + (Vector3)(finalDirection * dropDistance);

        GameObject instance = Instantiate(item.WorldPrefab, spawnPosition, Quaternion.identity);

        DroppedItem droppedItem = instance.GetComponent<DroppedItem>();
        if (droppedItem == null)
        {
            Debug.LogWarning("ItemDropSpawner: el WorldPrefab no tiene componente DroppedItem -> " + item.DisplayName);
            Destroy(instance);
            return null;
        }

        droppedItem.Setup(item, amount);
        droppedItem.BlockPickupFor(pickupBlockDuration);

        return droppedItem;
    }
}
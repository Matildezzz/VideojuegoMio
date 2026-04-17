using System;
using UnityEngine;

[Serializable]
public class RockDropEntry
{
    [SerializeField] private ItemData item;
    [SerializeField] private int minAmount = 1;
    [SerializeField] private int maxAmount = 1;

    public ItemData Item => item;
    public int MinAmount => minAmount;
    public int MaxAmount => maxAmount;

    public int GetRandomAmount()
    {
        int min = Mathf.Max(1, minAmount);
        int max = Mathf.Max(min, maxAmount);
        return UnityEngine.Random.Range(min, max + 1);
    }
}

public sealed class MineableRock : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHits = 3;
    [SerializeField] private int currentHits = 3;

    [Header("Loot")]
    [SerializeField] private RockDropEntry[] drops;
    [SerializeField] private ItemDropSpawner itemDropSpawner;
    [SerializeField] private float dropRadius = 0.5f;

    [Header("Opcional")]
    [SerializeField] private bool destroyWhenBroken = true;

    public bool TryMine(PlayerInventory playerInventory, Vector3 minerPosition)
    {
        if (currentHits <= 0)
        {
            return false;
        }

        currentHits--;

        if (currentHits > 0)
        {
            return true;
        }

        GiveLoot(playerInventory, minerPosition);

        if (destroyWhenBroken)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }

        return true;
    }

    private void GiveLoot(PlayerInventory playerInventory, Vector3 minerPosition)
    {
        if (drops == null || drops.Length == 0)
        {
            return;
        }

        for (int i = 0; i < drops.Length; i++)
        {
            RockDropEntry drop = drops[i];

            if (drop == null || drop.Item == null)
            {
                continue;
            }

            int amount = drop.GetRandomAmount();

            if (amount <= 0)
            {
                continue;
            }

            if (playerInventory == null)
            {
                SpawnDroppedItem(drop.Item, amount, minerPosition);
                continue;
            }

            int leftover = playerInventory.AddItemAndReturnLeftover(drop.Item, amount);

            if (leftover > 0)
            {
                SpawnDroppedItem(drop.Item, leftover, minerPosition);
            }
        }
    }

    private void SpawnDroppedItem(ItemData item, int amount, Vector3 minerPosition)
    {
        if (item == null || amount <= 0)
        {
            return;
        }

        if (itemDropSpawner == null)
        {
            Debug.LogWarning("MineableRock: falta asignar ItemDropSpawner en " + gameObject.name);
            return;
        }

        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * dropRadius;
        Vector3 spawnOrigin = transform.position + (Vector3)randomOffset;

        Vector2 direction = ((Vector2)(transform.position - minerPosition)).normalized;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector2.right;
        }

        itemDropSpawner.Spawn(item, amount, spawnOrigin, direction);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxHits < 1)
        {
            maxHits = 1;
        }

        if (currentHits < 1 || currentHits > maxHits)
        {
            currentHits = maxHits;
        }
    }
#endif
}
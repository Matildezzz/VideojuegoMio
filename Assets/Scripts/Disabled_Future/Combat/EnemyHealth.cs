using UnityEngine;

public sealed class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Identity")]
    [SerializeField] private string enemyId = "slime";

    [Header("Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth = 3;
    [SerializeField] private float knockbackForce = 2f;

    [Header("Loot")]
    [SerializeField] private ItemData dropItem;
    [SerializeField] private int dropAmount = 1;
    [SerializeField] private ItemDropSpawner itemDropSpawner;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    public string EnemyId => enemyId;
    public bool IsAlive => currentHealth > 0;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (itemDropSpawner == null)
        {
            itemDropSpawner = FindFirstObjectByType<ItemDropSpawner>();
        }

        if (maxHealth < 1)
        {
            maxHealth = 1;
        }

        currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);
    }

    public void TakeDamage(int amount, Vector2 hitDirection)
    {
        if (!IsAlive)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (rb != null && hitDirection.sqrMagnitude > 0.001f)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        SpawnLoot();

        if (QuestController.Instance != null)
        {
            QuestController.Instance.RegisterEnemyDefeated(enemyId);
        }

        Destroy(gameObject);
    }

    private void SpawnLoot()
    {
        if (dropItem == null)
        {
            return;
        }

        if (dropAmount <= 0)
        {
            return;
        }

        if (itemDropSpawner == null)
        {
            return;
        }

        Vector2 direction = Random.insideUnitCircle.normalized;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector2.right;
        }

        itemDropSpawner.Spawn(dropItem, dropAmount, transform.position, direction);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxHealth < 1)
        {
            maxHealth = 1;
        }

        currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);

        if (dropAmount < 0)
        {
            dropAmount = 0;
        }
    }
#endif
}
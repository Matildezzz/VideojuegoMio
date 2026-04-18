using UnityEngine;

public sealed class EnemyChaser2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseRange = 4f;
    [SerializeField] private float stopDistance = 0.7f;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform target;

    private EnemyHealth enemyHealth;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        if (target == null)
        {
            PlayerInventory playerInventory = FindFirstObjectByType<PlayerInventory>();

            if (playerInventory != null)
            {
                target = playerInventory.transform;
            }
        }
    }

    private void FixedUpdate()
    {
        if (PauseController.IsGamePaused)
        {
            StopMoving();
            return;
        }

        if (enemyHealth != null && !enemyHealth.IsAlive)
        {
            StopMoving();
            return;
        }

        if (target == null)
        {
            StopMoving();
            return;
        }

        Vector2 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;

        if (distance > chaseRange)
        {
            StopMoving();
            return;
        }

        if (distance <= stopDistance)
        {
            StopMoving();
            return;
        }

        Vector2 direction = toTarget.normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
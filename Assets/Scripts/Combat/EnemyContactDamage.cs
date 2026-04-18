using UnityEngine;

public sealed class EnemyContactDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float hitCooldown = 1f;

    private float nextHitTime = -1f;

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamage(collision.collider);
    }

    private void TryDamage(Component other)
    {
        if (other == null)
        {
            return;
        }

        if (Time.time < nextHitTime)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null || !playerHealth.IsAlive)
        {
            return;
        }

        Vector2 hitDirection = ((Vector2)(other.transform.position - transform.position)).normalized;

        if (hitDirection.sqrMagnitude < 0.001f)
        {
            hitDirection = Vector2.up;
        }

        playerHealth.TakeDamage(damage, hitDirection);
        nextHitTime = Time.time + hitCooldown;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (damage < 1)
        {
            damage = 1;
        }

        if (hitCooldown < 0.05f)
        {
            hitCooldown = 0.05f;
        }
    }
#endif
}
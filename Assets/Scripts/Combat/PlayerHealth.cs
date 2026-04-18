using System;
using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour, IDamageable
{
    public event Action<int, int> OnHealthChanged;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth = 5;

    [Header("Damage")]
    [SerializeField] private float invulnerabilityTime = 0.6f;
    [SerializeField] private float knockbackForce = 2f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerMovement playerMovement;

    private float invulnerableUntil = -1f;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
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

        if (Time.time < invulnerableUntil)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        invulnerableUntil = Time.time + invulnerabilityTime;

        if (rb != null && hitDirection.sqrMagnitude > 0.001f)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || !IsAlive)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log("PlayerHealth: el jugador ha muerto.");

        if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(true);
        }

        // Más adelante aquí puedes meter respawn, pantalla de muerte o volver a casa.
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxHealth < 1)
        {
            maxHealth = 1;
        }

        currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);
    }
#endif
}
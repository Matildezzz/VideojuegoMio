using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerHeartsUI : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Corazones")]
    [SerializeField] private Image[] heartImages = new Image[10];

    [Header("Sprites")]
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private Sprite halfHeartSprite;
    [SerializeField] private Sprite fullHeartSprite;

    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += OnHealthChanged;
        }

        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (playerHealth == null)
        {
            ClearVisuals();
            return;
        }

        for (int i = 0; i < heartImages.Length; i++)
        {
            Image heartImage = heartImages[i];

            if (heartImage == null)
            {
                continue;
            }

            heartImage.enabled = true;
            heartImage.sprite = GetSpriteForHeart(i);
        }
    }

    private Sprite GetSpriteForHeart(int heartIndex)
    {
        int hpStart = heartIndex * 2;
        int remainingHp = playerHealth.CurrentHealth - hpStart;

        if (remainingHp >= 2)
        {
            return fullHeartSprite;
        }

        if (remainingHp == 1)
        {
            return halfHeartSprite;
        }

        return emptyHeartSprite;
    }

    private void ClearVisuals()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
            {
                continue;
            }

            heartImages[i].enabled = emptyHeartSprite != null;
            heartImages[i].sprite = emptyHeartSprite;
        }
    }
}

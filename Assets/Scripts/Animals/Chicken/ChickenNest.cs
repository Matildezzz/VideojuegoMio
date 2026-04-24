using UnityEngine;
using UnityEngine.InputSystem;

public class ChickenNest : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite nestEmptySprite;
    [SerializeField] private Sprite nestWithEggSprite;

    [Header("Egg")]
    [SerializeField] private ItemData eggItemData;
    [SerializeField] private int eggAmount = 1;

    [Header("State")]
    [SerializeField] private bool hasEgg;
    [SerializeField] private int lastEggDay = -1;

    private IItemReceiver currentReceiver;
    private bool playerInside;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateVisual();
    }

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
        TryGenerateEgg(DayManager.CurrentDay);
    }

    private void Update()
    {
        if (!playerInside)
        {
            return;
        }

        if (!hasEgg)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectEgg();
        }
    }

    private void HandleNewDay(int currentDay)
    {
        TryGenerateEgg(currentDay);
    }

    private void TryGenerateEgg(int currentDay)
    {
        if (lastEggDay == currentDay)
        {
            return;
        }

        if (hasEgg)
        {
            return;
        }

        hasEgg = true;
        lastEggDay = currentDay;
        UpdateVisual();
    }

    private void CollectEgg()
    {
        if (currentReceiver == null)
        {
            return;
        }

        if (eggItemData == null)
        {
            Debug.LogWarning("El nido no tiene asignado el ItemData del huevo.");
            return;
        }

        bool added = currentReceiver.TryAddItem(eggItemData, eggAmount);

        if (!added)
        {
            Debug.Log("Inventario lleno. No se puede recoger el huevo.");
            return;
        }

        hasEgg = false;
        UpdateVisual();

        Debug.Log("Huevo recogido.");
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (hasEgg)
        {
            spriteRenderer.sprite = nestWithEggSprite;
        }
        else
        {
            spriteRenderer.sprite = nestEmptySprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IItemReceiver receiver = other.GetComponentInParent<IItemReceiver>();

        if (receiver == null)
        {
            return;
        }

        currentReceiver = receiver;
        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IItemReceiver receiver = other.GetComponentInParent<IItemReceiver>();

        if (receiver == null)
        {
            return;
        }

        if (receiver == currentReceiver)
        {
            currentReceiver = null;
            playerInside = false;
        }
    }
}
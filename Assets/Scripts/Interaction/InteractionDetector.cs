using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    private IProximityInfo proximityInfoInRange = null;
    public GameObject interactionIcon;

    private PlayerInventory playerInventory;

    private void Awake()
    {
        playerInventory = GetComponentInParent<PlayerInventory>();

        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }
    }

    private void Start()
    {
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
    }

    private void Update()
    {
        if (interactableInRange == null)
        {
            return;
        }

        if (PauseController.IsGamePaused)
        {
            return;
        }

        if (Keyboard.current == null || !Keyboard.current.gKey.wasPressedThisFrame)
        {
            return;
        }

        Component interactableComponent = interactableInRange as Component;

        if (interactableComponent == null)
        {
            return;
        }

        NPCFriendship friendship = interactableComponent.GetComponent<NPCFriendship>();

        if (friendship == null)
        {
            return;
        }

        string feedback;
        bool gifted = friendship.TryGiftSelectedItem(playerInventory, out feedback);

        if (!string.IsNullOrWhiteSpace(feedback))
        {
            Debug.Log(feedback);
        }

        if (gifted)
        {
            Debug.Log("Amistad actual con " + friendship.NpcId + ": " + friendship.CurrentHearts);

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.NotifySoldOrGiftedItem();
            }
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed || interactableInRange == null)
        {
            return;
        }

        interactableInRange.Interact();

        if (interactableInRange == null || !interactableInRange.CanInteract())
        {
            if (interactionIcon != null)
            {
                interactionIcon.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IProximityInfo proximityInfo = collision.GetComponent<IProximityInfo>();
        if (proximityInfo != null)
        {
            proximityInfoInRange = proximityInfo;
            proximityInfo.OnEnterProximity();
        }

        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null && interactable.CanInteract())
        {
            interactableInRange = interactable;

            if (interactionIcon != null)
            {
                interactionIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IProximityInfo proximityInfo = collision.GetComponent<IProximityInfo>();
        if (proximityInfo != null && proximityInfo == proximityInfoInRange)
        {
            proximityInfo.OnExitProximity();
            proximityInfoInRange = null;
        }

        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null && interactable == interactableInRange)
        {
            interactableInRange = null;

            if (interactionIcon != null)
            {
                interactionIcon.SetActive(false);
            }
        }
    }
}
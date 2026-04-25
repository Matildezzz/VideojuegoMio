using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    private IProximityInfo proximityInfoInRange = null;

    [Header("Icono antiguo")]
    public GameObject interactionIcon;

    [Header("Prompt de interaccion")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;

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
        SetPromptVisible(false);
    }

    private void Update()
    {
        RefreshPrompt();

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

            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast(feedback, gifted ? ToastType.Success : ToastType.Warning, gifted ? "Heart" : "Error");
            }
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
        RefreshPrompt();
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
        if (interactable == null)
        {
            interactable = collision.GetComponentInParent<IInteractable>();
        }

        if (interactable != null && interactable.CanInteract())
        {
            interactableInRange = interactable;
            RefreshPrompt();
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (interactableInRange != null)
        {
            return;
        }

        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable == null)
        {
            interactable = collision.GetComponentInParent<IInteractable>();
        }

        if (interactable != null && interactable.CanInteract())
        {
            interactableInRange = interactable;
            RefreshPrompt();
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
        if (interactable == null)
        {
            interactable = collision.GetComponentInParent<IInteractable>();
        }

        if (interactable != null && interactable == interactableInRange)
        {
            interactableInRange = null;
            SetPromptVisible(false);
        }
    }

    private void RefreshPrompt()
    {
        if (interactableInRange == null)
        {
            SetPromptVisible(false);
            return;
        }

        if (PauseController.IsGamePaused)
        {
            SetPromptVisible(false);
            return;
        }

        if (!interactableInRange.CanInteract())
        {
            SetPromptVisible(false);
            return;
        }

        if (promptText != null)
        {
            promptText.text = interactableInRange.InteractionText;
        }

        SetPromptVisible(true);
    }

    private void SetPromptVisible(bool visible)
    {
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(visible);
        }

        if (promptPanel != null)
        {
            promptPanel.SetActive(visible);
        }
        else if (promptText != null)
        {
            promptText.gameObject.SetActive(visible);
        }
    }
}

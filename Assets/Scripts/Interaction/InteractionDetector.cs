using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null; // Closest Interactable
    public GameObject interactionIcon;


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

    private void Start()
    {
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}

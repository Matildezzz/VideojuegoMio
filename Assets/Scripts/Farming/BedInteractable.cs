using UnityEngine;

public class BedInteractable : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private SleepConfirmationUI sleepUI;

    public string InteractionText => "E - Dormir";

    private void Awake()
    {
        if (sleepUI == null)
        {
            sleepUI = FindFirstObjectByType<SleepConfirmationUI>();
        }
    }

    public bool CanInteract()
    {
        return sleepUI != null && !sleepUI.IsOpen;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        sleepUI.Open();
    }
}

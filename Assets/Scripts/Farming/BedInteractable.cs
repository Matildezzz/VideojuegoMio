using UnityEngine;

public class BedInteractable : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private SleepConfirmationUI sleepUI;

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
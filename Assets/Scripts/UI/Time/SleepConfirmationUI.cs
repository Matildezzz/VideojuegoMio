using UnityEngine;

public class SleepConfirmationUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panel;
    [SerializeField] private SleepManager sleepManager;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (sleepManager == null)
        {
            sleepManager = FindFirstObjectByType<SleepManager>();
        }
    }

    private void Start()
    {
        SetOpen(false);
    }

    public void Open()
    {
        SetOpen(true);
    }

    public void Close()
    {
        SetOpen(false);
    }

    private void SetOpen(bool open)
    {
        if (panel == null)
        {
            Debug.LogWarning("SleepConfirmationUI: panel no asignado.");
            return;
        }

        panel.SetActive(open);
        PauseController.SetPause(open);
    }

    public void OnConfirmSleep()
    {
        SetOpen(false);

        if (sleepManager == null)
        {
            Debug.LogWarning("SleepConfirmationUI: SleepManager no asignado.");
            return;
        }

        sleepManager.SleepUntilMorning();
    }

    public void OnCancelSleep()
    {
        SetOpen(false);
    }
}
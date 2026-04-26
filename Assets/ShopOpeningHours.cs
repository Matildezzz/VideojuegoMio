using UnityEngine;

public sealed class ShopOpeningHours : MonoBehaviour
{
    [Header("Horario")]
    [Range(0, 23)] [SerializeField] private int openHour = 8;
    [Range(0, 59)] [SerializeField] private int openMinute = 0;
    [Range(0, 23)] [SerializeField] private int closeHour = 20;
    [Range(0, 59)] [SerializeField] private int closeMinute = 0;

    [Header("Mensajes")]
    [SerializeField] private string closedMessage = "La tienda está cerrada. Vuelve entre las 08:00 y las 20:00.";
    [SerializeField] private string openedMessage = "La tienda ha abierto.";

    [Header("Visual opcional")]
    [SerializeField] private GameObject openVisual;
    [SerializeField] private GameObject closedVisual;

    [Header("Opciones")]
    [SerializeField] private bool startOpenUntilTimeControllerSyncs = true;

    public bool IsCurrentlyOpen { get; private set; }
    public string ClosedMessage => closedMessage;
    public int OpenHour => openHour;
    public int CloseHour => closeHour;

    private void Awake()
    {
        IsCurrentlyOpen = startOpenUntilTimeControllerSyncs;
        RefreshVisuals();
    }

    public void ApplyTime(int hour, int minute, bool showMessage)
    {
        bool shouldBeOpen = IsOpenAt(hour, minute);
        SetOpenState(shouldBeOpen, showMessage);
    }

    public bool IsOpenAt(int hour, int minute)
    {
        int current = ToMinutes(hour, minute);
        int open = ToMinutes(openHour, openMinute);
        int close = ToMinutes(closeHour, closeMinute);

        if (open == close)
        {
            return true;
        }

        if (open < close)
        {
            return current >= open && current < close;
        }

        return current >= open || current < close;
    }

    public void SetOpenState(bool open, bool showMessage)
    {
        bool changed = IsCurrentlyOpen != open;
        IsCurrentlyOpen = open;

        RefreshVisuals();

        if (!showMessage || !changed)
        {
            return;
        }

        string message = open ? openedMessage : closedMessage;

        if (!string.IsNullOrWhiteSpace(message) && ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message);
        }
    }

    private void RefreshVisuals()
    {
        if (openVisual != null)
        {
            openVisual.SetActive(IsCurrentlyOpen);
        }

        if (closedVisual != null)
        {
            closedVisual.SetActive(!IsCurrentlyOpen);
        }
    }

    private int ToMinutes(int hour, int minute)
    {
        return Mathf.Clamp(hour, 0, 23) * 60 + Mathf.Clamp(minute, 0, 59);
    }
}
using TMPro;
using UnityEngine;

public class ClockUI : MonoBehaviour
{
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private string dayPrefix = "Día";

    private void Awake()
    {
        if (clockText == null)
        {
            clockText = GetComponent<TMP_Text>();
        }
    }

    private void Start()
    {
        if (TimeManager.Instance == null)
        {
            Debug.LogWarning("ClockUI: no se encontró TimeManager en la escena.");
            return;
        }

        TimeManager.Instance.OnTimeChanged += HandleTimeChanged;
        RefreshClock();
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= HandleTimeChanged;
        }
    }

    private void HandleTimeChanged(int day, int hour, int minute)
    {
        UpdateClock(day, hour, minute);
    }

    private void RefreshClock()
    {
        UpdateClock(
            TimeManager.Instance.Day,
            TimeManager.Instance.Hour,
            TimeManager.Instance.Minute
        );
    }

    private void UpdateClock(int day, int hour, int minute)
    {
        if (clockText == null)
        {
            return;
        }

        clockText.text = dayPrefix + " " + day + " - " + hour.ToString("00") + ":" + minute.ToString("00");
    }
}
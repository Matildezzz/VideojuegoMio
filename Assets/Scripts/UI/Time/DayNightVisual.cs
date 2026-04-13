using UnityEngine;
using UnityEngine.UI;

public class DayNightVisual : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] private Image dayNightOverlay;

    [Header("Colores")]
    [SerializeField] private Color dayColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color sunsetColor = new Color(1f, 0.55f, 0.25f, 0.15f);
    [SerializeField] private Color duskColor = new Color(0.35f, 0.3f, 0.5f, 0.25f);
    [SerializeField] private Color nightColor = new Color(0.05f, 0.1f, 0.2f, 0.45f);

    private void Start()
    {
        if (dayNightOverlay == null)
        {
            Debug.LogWarning("DayNightVisual: dayNightOverlay no asignado.");
            return;
        }

        RefreshVisual();

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged += HandleTimeChanged;
        }
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
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (TimeManager.Instance == null || dayNightOverlay == null)
        {
            return;
        }

        dayNightOverlay.color = GetCurrentOverlayColor();
    }

    private Color GetCurrentOverlayColor()
    {
        int totalMinutes = TimeManager.Instance.Hour * 60 + TimeManager.Instance.Minute;

        int dayStart = 6 * 60;
        int sunsetStart = 18 * 60;
        int duskStart = 20 * 60;
        int nightStart = 22 * 60;

        if (totalMinutes >= dayStart && totalMinutes < sunsetStart)
        {
            return dayColor;
        }

        if (totalMinutes >= sunsetStart && totalMinutes < duskStart)
        {
            float t = (totalMinutes - sunsetStart) / (float)(duskStart - sunsetStart);
            return Color.Lerp(dayColor, sunsetColor, t);
        }

        if (totalMinutes >= duskStart && totalMinutes < nightStart)
        {
            float t = (totalMinutes - duskStart) / (float)(nightStart - duskStart);
            return Color.Lerp(sunsetColor, duskColor, t);
        }

        if (totalMinutes >= nightStart)
        {
            return nightColor;
        }

        float dawnT = totalMinutes / (float)dayStart;
        return Color.Lerp(nightColor, dayColor, dawnT);
    }
}
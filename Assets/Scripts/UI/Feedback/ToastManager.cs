using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ToastType
{
    Normal,
    Success,
    Warning,
    Error
}

public sealed class ToastManager : MonoBehaviour
{
    public static ToastManager Instance { get; private set; }

    [Header("Referencias UI")]
    [SerializeField] private GameObject toastPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text toastText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;

    [Header("Iconos opcionales")]
    [SerializeField] private Sprite normalIcon;
    [SerializeField] private Sprite successIcon;
    [SerializeField] private Sprite warningIcon;
    [SerializeField] private Sprite errorIcon;

    [Header("Colores")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color successTextColor = Color.white;
    [SerializeField] private Color warningTextColor = Color.yellow;
    [SerializeField] private Color errorTextColor = Color.red;

    [SerializeField] private Color normalBackgroundColor = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private Color successBackgroundColor = new Color(0f, 0.35f, 0.1f, 0.85f);
    [SerializeField] private Color warningBackgroundColor = new Color(0.55f, 0.35f, 0f, 0.85f);
    [SerializeField] private Color errorBackgroundColor = new Color(0.45f, 0f, 0f, 0.85f);

    [Header("Tiempos")]
    [SerializeField] private float visibleSeconds = 2f;
    [SerializeField] private float fadeSeconds = 0.15f;

    [Header("Cola de mensajes")]
    [SerializeField] private int maxQueuedToasts = 5;

    private readonly Queue<ToastMessage> pendingToasts = new Queue<ToastMessage>();
    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (canvasGroup == null && toastPanel != null)
        {
            canvasGroup = toastPanel.GetComponent<CanvasGroup>();
        }

        if (toastPanel != null)
        {
            toastPanel.SetActive(false);
        }
    }

    public void ShowToast(string message)
    {
        ShowToast(message, ToastType.Normal, string.Empty);
    }

    public void ShowToast(string message, ToastType toastType)
    {
        ShowToast(message, toastType, string.Empty);
    }

    public void ShowToast(string message, ToastType toastType, string soundName)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (toastPanel == null || toastText == null)
        {
            Debug.Log(message);
            PlaySound(soundName);
            return;
        }

        while (pendingToasts.Count >= Mathf.Max(1, maxQueuedToasts))
        {
            pendingToasts.Dequeue();
        }

        pendingToasts.Enqueue(new ToastMessage(message, toastType, soundName));

        if (currentRoutine == null)
        {
            currentRoutine = StartCoroutine(ProcessToastQueue());
        }
    }

    private IEnumerator ProcessToastQueue()
    {
        while (pendingToasts.Count > 0)
        {
            ToastMessage toast = pendingToasts.Dequeue();
            ApplyToast(toast);

            toastPanel.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                yield return FadeTo(1f);
            }

            yield return new WaitForSecondsRealtime(visibleSeconds);

            if (canvasGroup != null)
            {
                yield return FadeTo(0f);
            }

            toastPanel.SetActive(false);
        }

        currentRoutine = null;
    }

    private void ApplyToast(ToastMessage toast)
    {
        toastText.text = toast.Message;
        toastText.color = GetTextColor(toast.Type);

        if (backgroundImage != null)
        {
            backgroundImage.color = GetBackgroundColor(toast.Type);
        }

        if (iconImage != null)
        {
            Sprite icon = GetIcon(toast.Type);
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        PlaySound(toast.SoundName);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeSeconds);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    private void PlaySound(string soundName)
    {
        if (string.IsNullOrWhiteSpace(soundName))
        {
            return;
        }

        SoundEffectManager.Play(soundName, true);
    }

    private Color GetTextColor(ToastType toastType)
    {
        switch (toastType)
        {
            case ToastType.Success:
                return successTextColor;
            case ToastType.Warning:
                return warningTextColor;
            case ToastType.Error:
                return errorTextColor;
            default:
                return normalTextColor;
        }
    }

    private Color GetBackgroundColor(ToastType toastType)
    {
        switch (toastType)
        {
            case ToastType.Success:
                return successBackgroundColor;
            case ToastType.Warning:
                return warningBackgroundColor;
            case ToastType.Error:
                return errorBackgroundColor;
            default:
                return normalBackgroundColor;
        }
    }

    private Sprite GetIcon(ToastType toastType)
    {
        switch (toastType)
        {
            case ToastType.Success:
                return successIcon;
            case ToastType.Warning:
                return warningIcon;
            case ToastType.Error:
                return errorIcon;
            default:
                return normalIcon;
        }
    }

    private readonly struct ToastMessage
    {
        public readonly string Message;
        public readonly ToastType Type;
        public readonly string SoundName;

        public ToastMessage(string message, ToastType type, string soundName)
        {
            Message = message;
            Type = type;
            SoundName = soundName;
        }
    }
}

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestBannerUI : MonoBehaviour
{
    [SerializeField] private GameObject bannerRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private Image iconImage;

    [Header("Tiempos")]
    [SerializeField] private float visibleSeconds = 2.5f;
    [SerializeField] private float fadeSeconds = 0.2f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (canvasGroup == null && bannerRoot != null)
        {
            canvasGroup = bannerRoot.GetComponent<CanvasGroup>();
        }

        if (bannerRoot != null)
        {
            bannerRoot.SetActive(false);
        }
    }

    public void ShowBanner(string title, string subtitle, Sprite icon = null)
    {
        if (bannerRoot == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (subtitleText != null)
        {
            subtitleText.text = subtitle;
        }

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        bannerRoot.SetActive(true);

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

        bannerRoot.SetActive(false);
        currentRoutine = null;
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
}

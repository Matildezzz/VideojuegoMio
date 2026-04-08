using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private CinemachineCamera vcam;

    private CinemachineFollow follow;
    private Vector3 originalDamping;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (canvasGroup == null)
        {
            Debug.LogError("ScreenFader: canvasGroup no está asignado.");
            return;
        }

        if (vcam == null)
        {
            Debug.LogError("ScreenFader: vcam no está asignada.");
            return;
        }

        follow = vcam.GetComponent<CinemachineFollow>();

        if (follow == null)
        {
            Debug.LogError("ScreenFader: no se encontró CinemachineFollow en la cámara.");
            return;
        }

        originalDamping = follow.TrackerSettings.PositionDamping;
    }

    async Task Fade(float targetTransparency)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, targetTransparency, t / fadeDuration);
            await Task.Yield();
        }

        canvasGroup.alpha = targetTransparency;
    }

    public async Task FadeOut()
    {
        SetDamping(Vector3.zero);
        await Fade(1f);
    }

    public async Task FadeIn()
    {
        await Fade(0f);
        RestoreDamping();
    }

    void SetDamping(Vector3 d)
    {
        if (follow == null) return;

        var settings = follow.TrackerSettings;
        settings.PositionDamping = d;
        follow.TrackerSettings = settings;
    }

    void RestoreDamping()
    {
        if (follow == null) return;

        var settings = follow.TrackerSettings;
        settings.PositionDamping = originalDamping;
        follow.TrackerSettings = settings;
    }
}
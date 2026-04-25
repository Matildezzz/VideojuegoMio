using System.Threading.Tasks;
using UnityEngine;

public class SleepManager : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private int wakeUpHour = 6;
    [SerializeField] private int wakeUpMinute = 0;

    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform wakeUpPoint;

    private bool isSleeping;

    private void Awake()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    public async void SleepUntilMorning()
    {
        if (isSleeping)
        {
            return;
        }

        if (TimeManager.Instance == null)
        {
            Debug.LogWarning("SleepManager: no existe TimeManager en la escena.");
            return;
        }

        isSleeping = true;
        PauseController.SetPause(true);

        if (ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeOut();
        }

        int nextDay = TimeManager.Instance.Day + 1;
        TimeManager.Instance.SetTime(nextDay, wakeUpHour, wakeUpMinute);

        if (playerTransform != null && wakeUpPoint != null)
        {
            playerTransform.position = wakeUpPoint.position;
        }

        if (ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeIn();
        }

        PauseController.SetPause(false);
        isSleeping = false;

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.NotifySlept();
        }
    }
}
using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static bool IsGamePaused { get; private set; } = false;

    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;
        Time.timeScale = pause ? 0f : 1f;
    }

    private void OnDestroy()
    {
        if (IsGamePaused)
        {
            SetPause(false);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool openWithEscape = true;

    private void Start()
    {
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null || menuCanvas == null)
        {
            return;
        }

        bool keyPressed = openWithEscape
            ? Keyboard.current.escapeKey.wasPressedThisFrame
            : Keyboard.current.tabKey.wasPressedThisFrame;

        if (keyPressed)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (!menuCanvas.activeSelf && PauseController.IsGamePaused)
        {
            return;
        }

        bool open = !menuCanvas.activeSelf;
        menuCanvas.SetActive(open);
        PauseController.SetPause(open);
    }

    public void ResumeGame()
    {
        if (menuCanvas == null)
        {
            return;
        }

        menuCanvas.SetActive(false);
        PauseController.SetPause(false);
    }

    public void BackToMainMenu()
    {
        PauseController.SetPause(false);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        PauseController.SetPause(false);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

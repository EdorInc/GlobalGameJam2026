using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public enum MenuState
    {
        Main,
        Controls
    }

    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject controlsMenu;

    private MenuState currentState;

    private void Start()
    {
        ShowMenu(MenuState.Main);
    }

    public void ShowMenu(MenuState newState)
    {
        // Disable all panels first
        mainMenu.SetActive(false);
        controlsMenu.SetActive(false);

        // Enable the selected one
        switch (newState)
        {
            case MenuState.Main:
                mainMenu.SetActive(true);
                break;

            case MenuState.Controls:
                controlsMenu.SetActive(true);
                break;

        }

        currentState = newState;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("RGB Scene");
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void OpenMainMenu()
    {
        ShowMenu(MenuState.Main);
    }

    public void OpenControls()
    {
        ShowMenu(MenuState.Controls);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

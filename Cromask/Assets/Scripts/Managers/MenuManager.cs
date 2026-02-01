using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public enum MenuState
    {
        Main,
        Controls,
        Confirm
    }

    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject confirmMenu;

    [Header("Player Toggles")]
    [SerializeField] private Toggle player1Toggle;
    [SerializeField] private Toggle player2Toggle;

    private MenuState currentState;

    private void Start()
    {
        ShowMenu(MenuState.Main);

        // Subscribe to toggle changes
        player1Toggle.onValueChanged.AddListener(_ => CheckStart());
        player2Toggle.onValueChanged.AddListener(_ => CheckStart());
    }

    private void CheckStart()
    {
        // If both toggles are ON, start the game
        if (player1Toggle.isOn && player2Toggle.isOn)
        {
            StartGame();
        }
    }

    public void ShowMenu(MenuState newState)
    {
        // Disable all panels first
        mainMenu.SetActive(false);
        controlsMenu.SetActive(false);
        confirmMenu.SetActive(false);

        // Enable the selected one
        switch (newState)
        {
            case MenuState.Main:
                mainMenu.SetActive(true);
                break;

            case MenuState.Controls:
                controlsMenu.SetActive(true);
                break;

            case MenuState.Confirm:
                confirmMenu.SetActive(true);
                break;

        }

        currentState = newState;
    }

    public void StartGame()
    {
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.Destroy();
        Debug.Log("Both players ready — starting game!");
        SceneManager.LoadScene("SceneTutorial");

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
    public void OpenConfirm()
    {
        ShowMenu(MenuState.Confirm);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using UnityEngine.InputSystem;

public static class GameEvents
{
    public static event Action OnGoToMenuRequested;

    public static event Action OnStartRequested;
    public static event Action OnRestartRequested;
    public static event Action OnExitRequested;

    public static event Action OnPauseRequested;
    public static event Action OnResumeRequested;

    public static void GoToMenuRequested() => OnGoToMenuRequested?.Invoke();

    public static void StartRequested() => OnStartRequested?.Invoke();
    public static void RestartRequested() => OnRestartRequested?.Invoke();
    
    public static void ExitRequested() => OnExitRequested?.Invoke();

    public static void PauseRequested() => OnPauseRequested?.Invoke();
    public static void ResumeRequested() => OnResumeRequested?.Invoke();
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] private string titleScene = "TitleScene";
    [SerializeField] private string gameScene = "GameScene";
    
    private PlayerInput player1Input;
    private PlayerInput player2Input;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetPlayerInput();
    }

    private void OnEnable()
    {
        GameEvents.OnGoToMenuRequested += LoadTitle;

        GameEvents.OnStartRequested += StartGame;
        GameEvents.OnRestartRequested += RestartGame;
        GameEvents.OnPauseRequested += PauseGame;
        GameEvents.OnResumeRequested += ResumeGame;
        GameEvents.OnExitRequested += QuitGame;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        GameEvents.OnGoToMenuRequested -= LoadTitle;

        GameEvents.OnStartRequested -= StartGame;
        GameEvents.OnRestartRequested -= RestartGame;
        GameEvents.OnPauseRequested -= PauseGame;
        GameEvents.OnResumeRequested -= ResumeGame;
        GameEvents.OnExitRequested -= QuitGame;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadTitle()
    {
        Debug.Log("Loading title scene...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleScene);
    }

    public void StartGame()
    {
        Debug.Log("Starting game...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
    }

    public void PauseGame()
    {
        Debug.Log("Pausing game...");
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Debug.Log("Resuming game...");
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetPlayerInput()
    {
        PlayerInput[] playerInputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);

        if (playerInputs.Length == 0)
        {
            Debug.LogWarning("No PlayerInput components found in this scene.");
        }
        else
        {
            if (playerInputs.Length == 1)
            {
                player1Input = playerInputs[0];
                Debug.LogWarning("Only one PlayerInput found. Player 2 will not be assigned.");
            }
            else
            {
                player1Input = playerInputs[0];
                player2Input = playerInputs[1];
            }
        }

        var keyboard = Keyboard.current;

        if (keyboard == null)
        {
            Debug.LogWarning("No keyboard detected. Control schemes not switched.");
            return;
        }

        if (player1Input != null)
            player1Input.SwitchCurrentControlScheme("Keyboard1", keyboard);

        if (player2Input != null)
            player2Input.SwitchCurrentControlScheme("Keyboard2", keyboard);
    }
}
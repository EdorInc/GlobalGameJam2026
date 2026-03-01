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
    [SerializeField] private PlayerInput player1Input;
    [SerializeField] private PlayerInput player2Input;

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

    private void Start()
    {
        var keyboard = Keyboard.current;

        player1Input.SwitchCurrentControlScheme("Keyboard1", keyboard);
        player2Input.SwitchCurrentControlScheme("Keyboard2", keyboard);
    }

    private void OnEnable()
    {
        GameEvents.OnGoToMenuRequested += LoadTitle;

        GameEvents.OnStartRequested += StartGame;
        GameEvents.OnRestartRequested += RestartGame;
        GameEvents.OnPauseRequested += PauseGame;
        GameEvents.OnResumeRequested += ResumeGame;
        GameEvents.OnExitRequested += QuitGame;
    }

    private void OnDisable()
    {
        GameEvents.OnGoToMenuRequested -= LoadTitle;

        GameEvents.OnStartRequested -= StartGame;
        GameEvents.OnRestartRequested -= RestartGame;
        GameEvents.OnPauseRequested -= PauseGame;
        GameEvents.OnResumeRequested -= ResumeGame;
        GameEvents.OnExitRequested -= QuitGame;
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
}
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

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Initial spawn Settings")]
    public Vector3 spawnPosition1 = new Vector3(-3, 1, 0);
    public Vector3 spawnPosition2 = new Vector3(3, 1, 0);

    private GameObject player1Instance;
    private GameObject player2Instance;

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

    private void OnEnable()
    {
        if (Instance != this) return;

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
        if (Instance != this) return;

        GameEvents.OnGoToMenuRequested -= LoadTitle;

        GameEvents.OnStartRequested -= StartGame;
        GameEvents.OnRestartRequested -= RestartGame;
        GameEvents.OnPauseRequested -= PauseGame;
        GameEvents.OnResumeRequested -= ResumeGame;
        GameEvents.OnExitRequested -= QuitGame;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameScene)
        {
            SpawnPlayers();
        }
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

    private void SpawnPlayers()
    {
        // Destruir jugadores anteriores si existen
        if (this.player1Instance != null)
        {
            Destroy(this.player1Instance);
        }
        if (this.player2Instance != null) 
        { 
            Destroy(this.player2Instance); 
        }

        var keyboard = Keyboard.current;

        if (keyboard == null)
        {
            Debug.LogWarning("No keyboard detected. Control schemes not switched.");
            return;
        }

        // --- PLAYER 1 ---
        // Al usar PlayerInput.Instantiate permitimos internamente que compartan controlador
        PlayerInput p1Input = PlayerInput.Instantiate(playerPrefab, 0, controlScheme: "Keyboard1", 0, keyboard);
        player1Instance = p1Input.transform.root.gameObject;
        
        player1Instance.transform.position = spawnPosition1;
        player1Instance.transform.rotation = Quaternion.identity;
        
        // ARREGLO FÍSICAS: Obligar a que el Rigidbody acepte instantáneamente su nueva coordenada
        Rigidbody rb1 = player1Instance.GetComponentInChildren<Rigidbody>();
        if (rb1 != null) rb1.position = spawnPosition1;
        p1Input.transform.localPosition = Vector3.zero;

        // --- PLAYER 2 ---
        PlayerInput p2Input = PlayerInput.Instantiate(playerPrefab, 1, controlScheme: "Keyboard2", 1, keyboard);
        player2Instance = p2Input.transform.root.gameObject;
        
        player2Instance.transform.position = spawnPosition2;
        player2Instance.transform.rotation = Quaternion.identity;
        
        Rigidbody rb2 = player2Instance.GetComponentInChildren<Rigidbody>();
        if (rb2 != null) rb2.position = spawnPosition2;
        p2Input.transform.localPosition = Vector3.zero;

        // --- SETUP EXTRAS ---
        player1Instance.GetComponentInChildren<CharacterStateController>().characterId = 0;
        player2Instance.GetComponentInChildren<CharacterStateController>().characterId = 1;

        // --- DYNAMIC SPLIT SCREEN ---
        DynamicSplitManager splitManager = GetComponent<DynamicSplitManager>();
        if (splitManager != null)
        {
            splitManager.SetupPlayers(player1Instance, player2Instance);
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using UnityEngine.InputSystem;

// TODO Move this to the EventManager
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
    [Tooltip("Name of the scene to load for the title screen.")]
    [SerializeField] private string titleScene = "TitleScene";
    [Tooltip("Names of the scenes to load for each level.")]
    [SerializeField] private string[] levelScenes = { "Level1", "Level2", "Level3" };

    [Header("Level Settings")]
    [SerializeField] private string currentLevelScene;

    private int currentLevelIndex = 0;

    [Header("Player Settings")]
    [Tooltip("Prefab to use when spawning player characters.")]
    [SerializeField] private GameObject playerPrefab;

    private Transform playerOneSpawn;
    private Transform playerTwoSpawn;

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

    private bool IsLevelScene(string sceneName)
    {
        if (levelScenes == null) return false;
        foreach (var level in levelScenes)
        {
            if (level == sceneName) return true;
        }
        return false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsLevelScene(scene.name))
        {
            currentLevelScene = scene.name;

            if (!ResolvePlayerSpawnsFromScene())
            {
                Debug.LogError($"Spawn points not found in scene '{scene.name}'.");
                return;
            }

            SpawnPlayers();
        }
    }

    public void LoadTitle()
    {
        Debug.Log("Loading title scene...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleScene);
    }

    public void LoadCurrentLevel()
    {
        if (levelScenes != null && levelScenes.Length > 0 && currentLevelIndex >= 0 && currentLevelIndex < levelScenes.Length)
        {
            SceneManager.LoadScene(levelScenes[currentLevelIndex]);
        }
        else
        {
            Debug.LogError("Invalid level index or no levels defined.");
        }
    }

    /// <summary>
    /// Loads the level at the specified index in the levelScenes array.
    /// </summary>
    /// <param name="levelIndex">The index of the level to load.</param>
    public void LoadLevel(int levelIndex)
    {
        if (levelScenes != null && levelScenes.Length > 0 && levelIndex >= 0 && levelIndex < levelScenes.Length)
        {
            currentLevelIndex = levelIndex;
            LoadCurrentLevel();
        }
        else
        {
            Debug.LogError($"Invalid level index {levelIndex}.");
        }
    }

    public void LoadNextLevel()
    {
        if (levelScenes != null && currentLevelIndex < levelScenes.Length - 1)
        {
            currentLevelIndex++;
            LoadCurrentLevel();
        }
        else
        {
            Debug.LogWarning("No more levels. Returning to title.");
            LoadTitle();
        }
    }

    public void LoadPreviousLevel()
    {
        if (levelScenes != null && currentLevelIndex > 0)
        {
            currentLevelIndex--;
            LoadCurrentLevel();
        }
        else
        {
            Debug.LogWarning("This was the first level. Returning to title.");
            LoadTitle();
        }
    }

    public void StartGame()
    {
        Debug.Log("Starting game...");
        Time.timeScale = 1f;
        LoadCurrentLevel();
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        Time.timeScale = 1f;
        LoadCurrentLevel();
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
        // Al usar PlayerInput.Instantiate permitimos internamente que compartan controlador.
        PlayerInput p1Input = PlayerInput.Instantiate(playerPrefab, 0, controlScheme: "Keyboard1", 0, keyboard);
        player1Instance = p1Input.transform.root.gameObject;

        player1Instance.transform.position = playerOneSpawn.position;
        player1Instance.transform.rotation = Quaternion.identity;

        // Obligar a que el Rigidbody acepte instantáneamente su nueva coordenada.
        Rigidbody rb1 = player1Instance.GetComponentInChildren<Rigidbody>();
        if (rb1 != null)
        {
            rb1.position = playerOneSpawn.position;
        }
        p1Input.transform.localPosition = Vector3.zero;

        // --- PLAYER 2 ---
        PlayerInput p2Input = PlayerInput.Instantiate(playerPrefab, 1, controlScheme: "Keyboard2", 1, keyboard);
        player2Instance = p2Input.transform.root.gameObject;

        player2Instance.transform.position = playerTwoSpawn.position;
        player2Instance.transform.rotation = Quaternion.identity;

        // Obligar a que el Rigidbody acepte instantáneamente su nueva coordenada.
        Rigidbody rb2 = player2Instance.GetComponentInChildren<Rigidbody>();
        if (rb2 != null)
        {
            rb2.position = playerTwoSpawn.position;
        }
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
        else
        {
            Debug.LogError("SplitManager missing");
        }
    }

    private bool ResolvePlayerSpawnsFromScene()
    {
        playerOneSpawn = null;
        playerTwoSpawn = null;

        var spawnPoints = FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);

        foreach (var spawnPoint in spawnPoints)
        {
            if (!spawnPoint.isActiveAndEnabled) continue;

            if (spawnPoint.PlayerSlot == PlayerSlot.Player1)
            {
                if (playerOneSpawn != null)
                {
                    Debug.LogWarning("Duplicate Player1 spawn found. Using first found.");
                    continue;
                }

                playerOneSpawn = spawnPoint.transform;
            }
            else if (spawnPoint.PlayerSlot == PlayerSlot.Player2)
            {
                if (playerTwoSpawn != null)
                {
                    Debug.LogWarning("Duplicate Player2 spawn found. Using first found.");
                    continue;
                }

                playerTwoSpawn = spawnPoint.transform;
            }
        }

        return playerOneSpawn != null && playerTwoSpawn != null;
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using UnityEngine.InputSystem;

// TODO Move this to the EventManager
public static class GameEvents
{
    public static event Action OnGoToMenuRequested;

    // TODO Provisional event to start a level from the level selection screen
    public static event Action<int> OnLevelSelectedRequested;

    public static event Action OnStartRequested;
    public static event Action OnRestartRequested;
    public static event Action OnExitRequested;

    public static event Action OnPauseRequested;
    public static event Action OnResumeRequested;

    public static void GoToMenuRequested() => OnGoToMenuRequested?.Invoke();

    public static void LevelSelectedRequested(int levelIndex) => OnLevelSelectedRequested?.Invoke(levelIndex);

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
    [Tooltip("Provisional name of the scene to load for the level selection.")]
    [SerializeField] private string levelSelectionScene = "LevelSelectionScene";
    [Tooltip("Names of the scenes to load for each level.")]
    [SerializeField] private string[] levelScenes = { "Level1", "Level2", "Level3" };

    [Header("Level Settings")]
    [SerializeField] private string currentLevelScene;

    private int currentLevelIndex = 0;

    [Header("Player Settings")]
    //[Tooltip("Prefab to use when spawning player characters.")]
    // [SerializeField] private GameObject playerPrefab;

    [Tooltip("Prefab to use when spawning player one character.")]
    [SerializeField] private GameObject playerPrefabOne;

    [Tooltip("Prefab to use when spawning player two character.")]
    [SerializeField] private GameObject playerPrefabTwo;

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

        // TODO Provisional bind to start game
        GameEvents.OnStartRequested += LoadLevelSelection;
        GameEvents.OnLevelSelectedRequested += LoadLevel;

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

        // TODO Provisional bind to start game
        GameEvents.OnStartRequested -= LoadLevelSelection;
        GameEvents.OnLevelSelectedRequested -= LoadLevel;

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

            AudioSystem.PlayMusic(AudioSystem.MusicLibrary?.mainTheme);
        }
        else if (scene.name == titleScene || scene.name == levelSelectionScene)
        {
            // If title or level selection scene, do nothing special.
            currentLevelScene = scene.name;
        }
        else
        {
            // If unknown scene log a warning and try to resolve spawns and spawn players anyway.
            Debug.LogWarning($"Scene '{scene.name}' is not registered as a level, title, or level selection scene. Attempting to resolve spawns and spawn players...");
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


    public void LoadLevelSelection()
    {
        Debug.Log("Loading level selection scene...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelSelectionScene);
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
        var gamepads = Gamepad.all;

        if (keyboard == null && gamepads.Count == 0)
        {
            Debug.LogWarning("No input devices detected.");
            Debug.LogWarning("No keyboard detected. Control schemes not switched.");
            return;
        }

        // Al usar PlayerInput.Instantiate permitimos internamente que compartan controlador.


        // --- PLAYER 1 ---

        PlayerInput p1Input;
        if (gamepads.Count > 0)
        {
            p1Input = PlayerInput.Instantiate(playerPrefabOne, 0, controlScheme: "Gamepad", 0, gamepads[0]);
        }
        else
        {
            p1Input = PlayerInput.Instantiate(playerPrefabOne, 0, controlScheme: "Keyboard1", 0, keyboard);
        }


        player1Instance = p1Input.transform.root.gameObject;
        player1Instance.transform.position = playerOneSpawn.position;
        player1Instance.transform.rotation = Quaternion.identity;

        Respawn player1Respawn = player1Instance.GetComponent<Respawn>();

        if (player1Respawn != null)
        {
            player1Respawn.respawnPosition = playerOneSpawn.position;
        }

        // Obligar a que el Rigidbody acepte instantáneamente su nueva coordenada.
        Rigidbody rb1 = player1Instance.GetComponentInChildren<Rigidbody>();
        if (rb1 != null)
        {
            rb1.position = playerOneSpawn.position;
        }
        p1Input.transform.localPosition = Vector3.zero;

        // --- PLAYER 2 ---

        PlayerInput p2Input;
        if (gamepads.Count > 1)
        {
            p2Input = PlayerInput.Instantiate(playerPrefabTwo, 1, controlScheme: "Gamepad", 1, gamepads[1]);
        }
        else
        {
            string schemeToUse = (gamepads.Count > 0) ? "Keyboard1" : "Keyboard2";
            p2Input = PlayerInput.Instantiate(playerPrefabTwo, 1, controlScheme: schemeToUse, 1, keyboard);
        }


        player2Instance = p2Input.transform.root.gameObject;

        player2Instance.transform.position = playerTwoSpawn.position;
        player2Instance.transform.rotation = Quaternion.identity;

        Respawn player2Respawn = player2Instance.GetComponent<Respawn>();

        if (player2Respawn != null)
        {
            player2Respawn.respawnPosition = playerTwoSpawn.position;
        }

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
    public void OnDeviceLost(PlayerInput player)
    {
        GameEvents.PauseRequested();
    }

    public void OnDeviceRegained(PlayerInput player)
    {
        GameEvents.ResumeRequested();
    }
}
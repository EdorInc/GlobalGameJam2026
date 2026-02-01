using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject confirmResetUI;

    [Header("Spawn Points")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    [SerializeField] private Transform red;
    [SerializeField] private Transform green;
    [SerializeField] private Transform blue;

    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (Transform child in transform)
        {
            switch (child.name)
            {
                case "Player1Spawn":
                    player1 = child;
                    break;

                case "Player2Spawn":
                    player2 = child;
                    break;

                case "RedSpawn":
                    red = child;
                    break;

                case "GreenSpawn":
                    green = child;
                    break;

                case "BlueSpawn":
                    blue = child;
                    break;
            }
        }
    }

    private void Start()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    public void TogglePause()
    {
        if (confirmResetUI != null && confirmResetUI.activeSelf)
        {
            CancelReset();
            return;
        }

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        // Pausar audio FMOD
        FMODUnity.RuntimeManager.PauseAllEvents(true);

        Debug.Log("Juego pausado");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // Reanudar audio FMOD
        FMODUnity.RuntimeManager.PauseAllEvents(false);

        Debug.Log("Juego reanudado");
    }

    public void ShowResetConfirmation()
    {
        if (confirmResetUI != null)
        {
            pauseMenuUI.SetActive(false);
            confirmResetUI.SetActive(true);
        }
    }
    public void CancelReset()
    {
        if (confirmResetUI != null)
        {
            confirmResetUI.SetActive(false);
            pauseMenuUI.SetActive(true);
        }
    }

    public void ConfirmReset()
    {
        if (confirmResetUI != null)
        {
            confirmResetUI.SetActive(false);
        }
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // Reanudar el tiempo antes del respawn
        isPaused = false;
        Time.timeScale = 1f;
        FMODUnity.RuntimeManager.PauseAllEvents(false);

        // Ejecutar el respawn
        if (SpawnManager.Instance != null)
        {
            if (player1 != null && player2 != null)
            {
                SpawnManager.Instance.RegisterPlayerSpawnPoints(player1, player2);
            }
            else
            {
                Debug.LogWarning("Missing player spawn points in Spawner.");
            }

            if (red != null && green != null && blue != null)
            {
                SpawnManager.Instance.RegisterMasksSpawnPoint(red, green, blue);
            }
            else
            {
                Debug.LogWarning("Missing mask spawn points in Spawner.");
            }
            SpawnManager.Instance.ForceRespawnAll();
        }
        else
        {
            Debug.LogWarning("SpawnManager.Instance es null");
        }

        Debug.Log("Reset completado");
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f; // Restaurar antes de cambiar de escena
        FMODUnity.RuntimeManager.PauseAllEvents(false);
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void SetCheckpoint(Transform checkpointTransform)
    {
        if (SpawnManager.Instance != null)
        {
            foreach (Transform child in checkpointTransform)
            {
                switch (child.name)
                {
                    case "Player1Spawn":
                        player1 = child;
                        break;

                    case "Player2Spawn":
                        player2 = child;
                        break;

                    case "RedSpawn":
                        red = child;
                        break;

                    case "GreenSpawn":
                        green = child;
                        break;

                    case "BlueSpawn":
                        blue = child;
                        break;
                }
            }

            Debug.Log("Checkpoint actualizado");
        }
        else
        {
            Debug.LogWarning("SpawnManager.Instance es null");
        }
    }
}

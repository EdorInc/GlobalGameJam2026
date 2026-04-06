using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GameScreenController : MonoBehaviour
{
    private VisualElement root;

    [Header("Time Settings")]
    [Range(1, 999)]
    [SerializeField] private int gameDuration = 300;

    [Header("References")]
    [SerializeField] private string pauseScreenName = "pause-ui";
    [SerializeField] private string winScreenName = "win-ui";
    [SerializeField] private string loseScreenName = "lose-ui";
    [SerializeField] private string videoScreenName = "tutorial-ui";

    [Header("HUD References")]
    [SerializeField] private string timerLabelName = "timer-label";

    private Label timerLabel;
    private float currentTime;

    [Header("Pause References")]
    [SerializeField] private string resumeButtonName = "resume-button";
    [SerializeField] private string restartButtonName = "restart-button";
    [SerializeField] private string menuButtonName = "quit-button";

    [Header("Win References")]
    [SerializeField] private string winRestartButtonName = "replay-button";
    [SerializeField] private string winMenuButtonName = "quit-button";

    [Header("Lose References")]
    [SerializeField] private string loseRestartButtonName = "restart-button";
    [SerializeField] private string loseMenuButtonName = "quit-button";

    [Header("Tutorial References")]
    [SerializeField] private RenderTexture videoTexture;

    [Header("Debug")]
    [SerializeField] private UIState initialState = UIState.Gameplay;

    private VisualElement pauseScreen;
    private VisualElement winScreen;
    private VisualElement loseScreen;
    private VisualElement videoScreen;

    private Button resumeButton;
    private Button restartButton;
    private Button menuButton;
    private Button winRestartButton;
    private Button winMenuButton;
    private Button loseRestartButton;
    private Button loseMenuButton;

    private UIState currentState = UIState.Gameplay;

    private enum UIState
    {
        Gameplay,
        Paused,
        Win,
        Lose
    }

    private void OnEnable()
    {
        EventManager.OnVictory += HasWon;
    }

    private void OnDisable()
    {
        EventManager.OnVictory -= HasWon;
    }

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        pauseScreen = root.Q<VisualElement>(pauseScreenName);

        if (pauseScreen == null)
        {
            Debug.LogError($"Pause screen with name '{pauseScreenName}' not found in the UI.");
            return;
        }

        winScreen = root.Q<VisualElement>(winScreenName);

        if (winScreen == null)
        {
            Debug.LogError($"Win screen with name '{winScreenName}' not found in the UI.");
            return;
        }

        loseScreen = root.Q<VisualElement>(loseScreenName);

        if (loseScreen == null)
        {
            Debug.LogError($"Lose screen with name '{loseScreenName}' not found in the UI.");
            return;
        }

        RegisterCallbacks();

        currentState = initialState;

        SetState(initialState);

        // HUD Timer
        timerLabel = root.Q<Label>(timerLabelName);
        if (timerLabel == null)
        {
            Debug.LogError($"Timer label with name '{timerLabelName}' not found in the UI.");
            return;
        }

        currentTime = gameDuration;
        UpdateTimer();


        //Video tutorial

        videoScreen = root.Q<VisualElement>(videoScreenName);
        if(videoScreen == null)
        {
            Debug.LogError($"Video screen with name '{videoScreenName}' not found in the UI.");
            return;
        }

        var videoElement = root.Q<Image>("tutorial-video");

        videoElement.image = videoTexture;
    }

    private void Update()
    {
        // This needs to be updated using the new Input System, but for now this is fine for testing.
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentState == UIState.Gameplay)
                SetState(UIState.Paused);
            else if (currentState == UIState.Paused)
                SetState(UIState.Gameplay);
        }

        if (currentState == UIState.Gameplay)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;

                if (currentTime <= 0)
                {
                    currentTime = 0;
                    UpdateTimer();
                    SetState(UIState.Lose);
                    return;
                }

                UpdateTimer();
            }
        }
    }

    private void UpdateTimer()
    {
        int seconds = Mathf.CeilToInt(currentTime);

        if (seconds < 0)
            seconds = 0;

        timerLabel.text = seconds.ToString("000");
    }

    private void HasWon()
    {
        if (currentState != UIState.Gameplay)
            return;

        SetState(UIState.Win);
    }

    private void SetState(UIState newState)
    {
        currentState = newState;

        pauseScreen.style.display = DisplayStyle.None;
        winScreen.style.display = DisplayStyle.None;
        loseScreen.style.display = DisplayStyle.None;

        switch (newState)
        {
            case UIState.Gameplay:
                GameEvents.ResumeRequested();
                break;

            case UIState.Paused:
                GameEvents.PauseRequested();
                pauseScreen.style.display = DisplayStyle.Flex;
                break;

            case UIState.Win:
                GameEvents.PauseRequested();
                winScreen.style.display = DisplayStyle.Flex;
                break;

            case UIState.Lose:
                GameEvents.PauseRequested();
                loseScreen.style.display = DisplayStyle.Flex;
                break;
        }
    }

    public void ShowWin()
    {
        SetState(UIState.Win);
    }

    public void ShowLose()
    {
        SetState(UIState.Lose);
    }

    private bool IsValidButton(string buttonName, Button button)
    {
        if (button == null)
        {
            Debug.LogError($"Button with name '{buttonName}' not found in the UI.");
            return false;
        }

        return true;
    }

    private void RegisterCallbacks()
    {
        // Pause
        resumeButton = pauseScreen.Q<Button>(resumeButtonName);
        if(!IsValidButton(resumeButtonName, resumeButton)) return;
        resumeButton.clicked += () => SetState(UIState.Gameplay);

        restartButton = pauseScreen.Q<Button>(restartButtonName);
        if(!IsValidButton(restartButtonName, restartButton)) return;
        restartButton.clicked += () => GameEvents.RestartRequested();

        menuButton = pauseScreen.Q<Button>(menuButtonName);
        if(!IsValidButton(menuButtonName, menuButton)) return;
        menuButton.clicked += () => GameEvents.GoToMenuRequested();

        // Win
        winRestartButton = winScreen.Q<Button>(winRestartButtonName);
        if(!IsValidButton(winRestartButtonName, winRestartButton)) return;
        winRestartButton.clicked += () => GameEvents.RestartRequested();

        winMenuButton = winScreen.Q<Button>(winMenuButtonName);
        if(!IsValidButton(winMenuButtonName, winMenuButton)) return;
        winMenuButton.clicked += () => GameEvents.GoToMenuRequested();

        // Lose
        loseRestartButton = loseScreen.Q<Button>(loseRestartButtonName);
        if(!IsValidButton(loseRestartButtonName, loseRestartButton)) return;
        loseRestartButton.clicked += () => GameEvents.RestartRequested();

        loseMenuButton = loseScreen.Q<Button>(loseMenuButtonName);
        if(!IsValidButton(loseMenuButtonName, loseMenuButton)) return;
        loseMenuButton.clicked += () => GameEvents.GoToMenuRequested();
    }
}
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private string playButtonName = "PlayButton";
    [SerializeField] private string quitButtonName = "ExitButton";

    private Button playButton;
    private Button quitButton;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        playButton = root.Q<Button>(playButtonName);

        if (playButton == null)
        {
            Debug.LogError($"Play button with name '{playButtonName}' not found in the UI.");
            return;
        }

        quitButton = root.Q<Button>(quitButtonName);

        if (quitButton == null)
        {
            Debug.LogError($"Quit button with name '{quitButtonName}' not found in the UI.");
            return;
        }

        playButton.clicked += () => GameEvents.StartRequested();
        quitButton.clicked += () => GameManager.Instance.QuitGame();
    }
}

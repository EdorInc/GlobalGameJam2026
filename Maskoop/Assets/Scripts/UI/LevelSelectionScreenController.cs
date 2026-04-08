using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class LevelSelectionScreenController : MonoBehaviour
{
    [SerializeField] private string levelButtonName = "LevelButton";

    private Button levelButton;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        levelButton = root.Q<Button>(levelButtonName);

        if (levelButton == null)
        {
            Debug.LogError($"Level button with name '{levelButtonName}' not found in the UI.");
            return;
        }

        //levelButton.clicked += () => GameEvents.StartRequested();
    }
}

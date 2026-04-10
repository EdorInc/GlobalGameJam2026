using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class LevelSelectionScreenController : MonoBehaviour
{
    [Header("Provisional Level Settings")]
    [SerializeField] private string levelButton01Name = "LevelButton01";
    [SerializeField] private string levelButton02Name = "LevelButton02";

    private Button levelButton01;
    private Button levelButton02;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        levelButton01 = root.Q<Button>(levelButton01Name);
        if (!BindButton(levelButton01, levelButton01Name, 0))
        {
            return;
        }

        levelButton02 = root.Q<Button>(levelButton02Name);
        if (!BindButton(levelButton02, levelButton02Name, 1))
        {
            return;
        }
    }

    private bool BindButton(Button button, string buttonName, int levelIndex)
    {
        if (button == null)
        {
            Debug.LogError($"Level button with name '{buttonName}' not found in the UI.");
            return false;
        }

        button.clicked += () => GameEvents.LevelSelectedRequested(levelIndex);
        return true;
    }
}

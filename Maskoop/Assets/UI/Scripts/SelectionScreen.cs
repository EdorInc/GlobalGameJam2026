using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[System.Serializable]
public struct TLevelData
{
    public string label;
    public Texture2D image;
    public string sceneName;
};

public class SelectionScreen : MonoBehaviour
{

    [SerializeField] TLevelData[] levels;

    private VisualElement root = null;
    private uint actualIndex = 0;

    public InputActionReference LevelSelectionDown;
    public InputActionReference LevelSelectionUp;
    public InputActionReference LevelSelectionLeft;
    public InputActionReference LevelSelectionRight;
    public InputActionReference LevelSelectionAccept;

    private Button backButton;
    private Button leftArrow;
    private Button rightArrow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        var levelScroll = root.Q<ScrollView>("level-scroll");
        leftArrow = root.Q<Button>("left-button");
        rightArrow = root.Q<Button>("right-button");

        backButton = root.Q<Button>("back-button");
        backButton.focusable = false;

        leftArrow.SetEnabled(false);
        leftArrow.visible = false;


        foreach ( var level in levels)
        {
            VisualElement container = new VisualElement();
            container.AddToClassList("level-container");

            Label label = new Label(level.label);
            label.AddToClassList("level-label");

            Image image = new Image();
            image.image = level.image;
            image.scaleMode = ScaleMode.ScaleAndCrop;
            image.AddToClassList("level-image");

            container.Add(label);
            container.Add(image);

            levelScroll.Add(container);
        }

        leftArrow.clicked += MoveLeft;
        rightArrow.clicked += MoveRight;

        levelScroll.scrollOffset += new Vector2(0,0);

        //Suscribirme a los eventos de los botones
        LevelSelectionLeft.action.Enable();
        LevelSelectionLeft.action.performed += OnLeftPressed;

        LevelSelectionRight.action.Enable();
        LevelSelectionRight.action.performed += OnRightPressed;

        LevelSelectionDown.action.Enable();
        LevelSelectionDown.action.performed += OnDownPressed;

        LevelSelectionUp.action.Enable();
        LevelSelectionUp.action.performed += OnUpPressed;

        LevelSelectionAccept.action.Enable();
        LevelSelectionAccept.action.performed += OnAccept;
    }

    private void MoveLeft()
    {
        if (actualIndex > 0)
        {
            actualIndex--;

            var levelScroll = root.Q<ScrollView>("level-scroll");
            var card = levelScroll.contentContainer.ElementAt(0);

            levelScroll.scrollOffset -= new Vector2(card.layout.width,0);

            if (actualIndex == 0)
            {
                var leftArrow = root.Q<Button>("left-button");
                leftArrow.SetEnabled(false);
                leftArrow.visible = false;
            }
            if (actualIndex != levels.Length - 1)
            {
                var rightArrow = root.Q<Button>("right-button");
                rightArrow.SetEnabled(true);
                rightArrow.visible = true;
            }

        }

    }

    private void MoveRight()
    {
        

        if (actualIndex < levels.Length - 1)
        {
            actualIndex++;

            var levelScroll = root.Q<ScrollView>("level-scroll");


            var card = levelScroll.contentContainer.ElementAt(0);

            levelScroll.scrollOffset += new Vector2(card.layout.width,0);

            Debug.Log("Me muevo");
            Debug.Log(actualIndex);
            Debug.Log(levelScroll.scrollOffset);

            if (actualIndex == levels.Length - 1)
            {
                var rightArrow = root.Q<Button>("right-button");
                rightArrow.SetEnabled(false);
                rightArrow.visible = false;
            }
            if (actualIndex > 0 )
            {
                var leftArrow = root.Q<Button>("left-button");
                leftArrow.SetEnabled(true);
                leftArrow.visible = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnLeftPressed(InputAction.CallbackContext context)
    {
        MoveLeft();
        backButton.focusable = false;
        backButton.Blur();
        leftArrow.Blur();
        rightArrow.Blur();
    }

    private void OnRightPressed(InputAction.CallbackContext context)
    {
        MoveRight();
        backButton.focusable = false;
        backButton.Blur();
        leftArrow.Blur();
        rightArrow.Blur();
    }

    private void OnDownPressed(InputAction.CallbackContext context)
    {
        backButton.focusable = true;
        backButton.Focus();
    }
    private void OnUpPressed(InputAction.CallbackContext context)
    {
        Debug.Log("arriba");
        backButton.focusable = false;
        backButton.Blur();
        leftArrow.Blur();
        rightArrow.Blur();
    }

    private void OnAccept(InputAction.CallbackContext context)
    {
        if (backButton.focusable)
        {
            SceneManager.LoadScene("TitleScene");
        }
        else
        {
            SceneManager.LoadScene(levels[actualIndex].sceneName);
        }
    }

    public void OnDisable()
    {
        //Suscribirme a los eventos de los botones
        LevelSelectionLeft.action.performed -= OnLeftPressed;
        LevelSelectionLeft.action.Disable();

        LevelSelectionRight.action.performed -= OnRightPressed;
        LevelSelectionRight.action.Disable();

        
        LevelSelectionDown.action.performed -= OnDownPressed;
        LevelSelectionDown.action.Disable();

        LevelSelectionUp.action.performed -= OnUpPressed;
        LevelSelectionUp.action.Disable();

        LevelSelectionAccept.action.performed -= OnAccept;
        LevelSelectionAccept.action.Disable();
    }


}

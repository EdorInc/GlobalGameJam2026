using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonsCanvasController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button[] buttons;

    [Header("Navigation Settings")]
    [SerializeField] private float inputCooldown = 0.2f;

    [Header("Visual Feedback")]
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    private int currentIndex = 0;
    private float lastInputTime;

    private void Start()
    {
        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogError("ButtonsCanvasController: No buttons assigned.");
            enabled = false;
            return;
        }

        SelectButton(0);
    }

    private void Update()
    {
        if (Time.unscaledTime - lastInputTime < inputCooldown)
            return;

        Vector2 navigation = Vector2.zero;

        // Gamepad input
        if (Gamepad.current != null)
        {
            navigation = Gamepad.current.leftStick.ReadValue();
            Vector2 dpad = Gamepad.current.dpad.ReadValue();

            if (dpad.sqrMagnitude > navigation.sqrMagnitude)
                navigation = dpad;

            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                buttons[currentIndex].onClick.Invoke();
                lastInputTime = Time.unscaledTime;
                return;
            }
        }

        // Keyboard input
        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed)
                navigation.y = 1f;
            else if (Keyboard.current.downArrowKey.isPressed)
                navigation.y = -1f;

            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                buttons[currentIndex].onClick.Invoke();
                lastInputTime = Time.unscaledTime;
                return;
            }
        }

        // Vertical navigation
        if (navigation.y > 0.5f)
        {
            NavigateUp();
        }
        else if (navigation.y < -0.5f)
        {
            NavigateDown();
        }
    }

    private void NavigateUp()
    {
        currentIndex = (currentIndex - 1 + buttons.Length) % buttons.Length;
        SelectButton(currentIndex);
        lastInputTime = Time.unscaledTime;
    }

    private void NavigateDown()
    {
        currentIndex = (currentIndex + 1) % buttons.Length;
        SelectButton(currentIndex);
        lastInputTime = Time.unscaledTime;
    }

    private void SelectButton(int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].transform.localScale = Vector3.one;

            Image image = buttons[i].GetComponent<Image>();
            if (image != null)
                image.color = normalColor;
        }

        currentIndex = index;

        Button selectedButton = buttons[currentIndex];
        selectedButton.transform.localScale = Vector3.one * selectedScale;

        Image selectedImage = selectedButton.GetComponent<Image>();
        if (selectedImage != null)
            selectedImage.color = selectedColor;

        selectedButton.Select();
        EventSystem.current.SetSelectedGameObject(selectedButton.gameObject);
    }

    private void OnEnable()
    {
        if (buttons != null && buttons.Length > 0)
        {
            currentIndex = 0;
            SelectButton(0);
        }
    }
}

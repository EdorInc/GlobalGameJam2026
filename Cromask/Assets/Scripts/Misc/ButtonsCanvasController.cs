using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonsCanvasController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button exitButton;

    [Header("Navigation Settings")]
    [SerializeField] private float inputCooldown = 0.2f;

    [Header("Visual Feedback")]
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    private Button[] buttons;
    private int currentIndex = 0;
    private float lastInputTime;

    private void Start()
    {
        buttons = new Button[] { startButton, controlsButton, exitButton };
        SelectButton(0);
    }

    private void Update()
    {
        if (Time.unscaledTime - lastInputTime < inputCooldown)
            return;

        Vector2 navigation = Vector2.zero;

        // Input de Gamepad
        if (Gamepad.current != null)
        {
            navigation = Gamepad.current.leftStick.ReadValue();
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            if (dpad.sqrMagnitude > navigation.sqrMagnitude)
                navigation = dpad;

            // Botón A para seleccionar
            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                buttons[currentIndex].onClick.Invoke();
                lastInputTime = Time.unscaledTime;
                return;
            }
        }

        // Input de Teclado (flechas)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed)
                navigation.y = 1f;
            else if (Keyboard.current.downArrowKey.isPressed)
                navigation.y = -1f;

            // Enter para seleccionar
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                buttons[currentIndex].onClick.Invoke();
                lastInputTime = Time.unscaledTime;
                return;
            }
        }

        // Navegación vertical
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
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = buttons.Length - 1;

        SelectButton(currentIndex);
        lastInputTime = Time.unscaledTime;
    }

    private void NavigateDown()
    {
        currentIndex++;
        if (currentIndex >= buttons.Length)
            currentIndex = 0;

        SelectButton(currentIndex);
        lastInputTime = Time.unscaledTime;
    }

    private void SelectButton(int index)
    {
        // Resetear todos los botones al estado normal
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].transform.localScale = Vector3.one;

            Image image = buttons[i].GetComponent<Image>();
            if (image != null)
                image.color = normalColor;
        }

        currentIndex = index;
        buttons[currentIndex].transform.localScale = Vector3.one * selectedScale;

        var selectedImage = buttons[currentIndex].GetComponent<Image>();
        if (selectedImage != null)
            selectedImage.color = selectedColor;

        buttons[currentIndex].Select();
        EventSystem.current.SetSelectedGameObject(buttons[currentIndex].gameObject);
    }

    private void OnEnable()
    {
        // Resetear al primer botón cuando se active el canvas
        currentIndex = 0;
        if (buttons != null && buttons.Length > 0)
            SelectButton(0);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BackButtonController : MonoBehaviour
{
    [SerializeField] private Button backButton;

    private void Update()
    {
        if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            backButton.onClick.Invoke();
        }

        // También con Escape del teclado
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            backButton.onClick.Invoke();
        }
    }
}

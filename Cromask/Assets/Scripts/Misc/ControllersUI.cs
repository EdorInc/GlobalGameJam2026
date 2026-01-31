using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class ControllersUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject player1Panel;
    [SerializeField] private GameObject player2Panel;

    [Header("Confirm Toggles")]
    [SerializeField] private Toggle player1ConfirmToggle;
    [SerializeField] private Toggle player2ConfirmToggle;

    [Header("Player Prefabs/References")]
    [SerializeField] private GameObject player1Character;
    [SerializeField] private GameObject player2Character;

    private Gamepad player1Gamepad;
    private Gamepad player2Gamepad;

    private bool player1Assigned = false;
    private bool player2Assigned = false;

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void Update()
    {
        // Detectar input de cualquier gamepad no asignado
        foreach (var gamepad in Gamepad.all)
        {
            // Verificar si este gamepad ya está asignado
            if (gamepad == player1Gamepad || gamepad == player2Gamepad)
                continue;

            // Detectar cualquier botón presionado en el gamepad
            if (IsAnyButtonPressed(gamepad))
            {
                AssignGamepad(gamepad);
            }
        }
    }

    private bool IsAnyButtonPressed(Gamepad gamepad)
    {
        foreach (var control in gamepad.allControls)
        {
            if (control is ButtonControl button && button.isPressed)
            {
                return true;
            }
        }
        return false;
    }

    private void AssignGamepad(Gamepad gamepad)
    {
        if (!player1Assigned)
        {
            player1Gamepad = gamepad;
            player1Assigned = true;
            player1ConfirmToggle.isOn = true;

            PlayerInput playerInput = player1Character.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.SwitchCurrentControlScheme(gamepad);
            }

            HighlightPanel(player1Panel, true);
            Debug.Log("Player 1 asignado: " + gamepad.displayName);
        }
        else if (!player2Assigned)
        {
            player2Gamepad = gamepad;
            player2Assigned = true;
            player2ConfirmToggle.isOn = true;

            PlayerInput playerInput = player2Character.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.SwitchCurrentControlScheme(gamepad);
            }

            HighlightPanel(player2Panel, true);
            Debug.Log("Player 2 asignado: " + gamepad.displayName);
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad gamepad)
        {
            if (change == InputDeviceChange.Disconnected)
            {
                HandleGamepadDisconnected(gamepad);
            }
        }
    }

    private void HandleGamepadDisconnected(Gamepad gamepad)
    {
        if (gamepad == player1Gamepad)
        {
            player1Gamepad = null;
            player1Assigned = false;
            player1ConfirmToggle.isOn = false;
            HighlightPanel(player1Panel, false);
            Debug.Log("Player 1 desconectado");
        }
        else if (gamepad == player2Gamepad)
        {
            player2Gamepad = null;
            player2Assigned = false;
            player2ConfirmToggle.isOn = false;
            HighlightPanel(player2Panel, false);
            Debug.Log("Player 2 desconectado");
        }
    }

    private void HighlightPanel(GameObject panel, bool highlight)
    {
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = highlight ? Color.green : Color.white;
        }
    }
}

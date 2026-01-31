using UnityEngine;
using UnityEngine.InputSystem;

public class RegisterController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Gamepad playerGamepad;
    void Start()
    {
        PlayerInput input = GetComponent<PlayerInput>();
        if (!input) 
        {
            Debug.Log("PlayerInput component not found.");
            return;
        }

        if (input.devices.Count > 0 && input.devices[0] is Gamepad pad)
        {
            playerGamepad = pad;
            Debug.Log("Registered gamepad: " + pad.displayName);
        }
        else
        {
            Debug.LogWarning("No gamepad assigned to this PlayerInput");
        }
    }

    public Gamepad GetPlayerGamepad()
    {
        return playerGamepad;
    }
}

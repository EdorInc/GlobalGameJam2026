using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private RigidbodyCharacterController charController;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction jumpAction;

    private void Awake()
    {
        charController = GetComponent<RigidbodyCharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
    }

    private void FixedUpdate()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();

        int vertical = Mathf.RoundToInt(move.y);
        int horizontal = Mathf.RoundToInt(move.x);

        bool jump = jumpAction.IsPressed();

        charController.ForwardInput = horizontal;
        charController.SideInput = vertical;
        charController.JumpInput = jump;
    }
}
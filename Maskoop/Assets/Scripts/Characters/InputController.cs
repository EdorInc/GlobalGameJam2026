using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private RigidbodyCharacterController charController;
    private Grab grabComponent;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction jumpAction;

    private InputAction grabAction;
    private InputAction dropAction;

    private void Awake()
    {
        charController = GetComponent<RigidbodyCharacterController>();
        grabComponent = GetComponent<Grab>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        grabAction = playerInput.actions["Grab"];
        dropAction = playerInput.actions["Drop"];
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

        bool grab = grabAction.IsPressed();
        bool drop = dropAction.IsPressed();

        if (grab) grabComponent.GrabObject();
        else if (drop) grabComponent.DropObject();

    }
}
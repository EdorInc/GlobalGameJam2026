using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private RigidbodyCharacterController charController;
    private Grab grabComponent;
    private Throw throwComponent;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction jumpAction;

    private InputAction grabAction;
    private InputAction dropAction;

    private InputAction throwAction;

    private void Awake()
    {
        charController = GetComponent<RigidbodyCharacterController>();
        grabComponent = GetComponent<Grab>();
        throwComponent = GetComponent<Throw>();

        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        grabAction = playerInput.actions["Grab"];
        dropAction = playerInput.actions["Drop"];
        throwAction = playerInput.actions["Throw"];

        throwAction.started += OnThrowStarted;
        throwAction.canceled += OnThrowReleased;
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

    private void OnThrowStarted(InputAction.CallbackContext ctx)
    {
        throwComponent.ChargeObject();
    }

    private void OnThrowReleased(InputAction.CallbackContext ctx)
    {
        throwComponent.ThrowObject();
    }
}
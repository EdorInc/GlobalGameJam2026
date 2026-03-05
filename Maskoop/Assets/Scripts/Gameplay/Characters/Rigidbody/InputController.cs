using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class InputController : MonoBehaviour
{
    [Header("References")]
    private CharacterMovementController playerController;
    private Grab grabComponent;
    private Throw throwComponent;
    private Equip equipableManager;

    private void Awake()
    {
        playerController = GetComponent<CharacterMovementController>();
        grabComponent = GetComponent<Grab>();
        equipableManager = GetComponent<Equip>();
        throwComponent = GetComponent<Throw>();
    }

    public void OnMove(CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 moveDir = ctx.ReadValue<Vector2>();

            playerController.ForwardInput = moveDir.x;
            playerController.SideInput = moveDir.y;

            //VibrationManager.Instance.RumblePulse(registerController.GetPlayerGamepad(), 0.1f, 0.1f, 0.1f);
        }
        else if (ctx.canceled)
        {
            playerController.ForwardInput = 0;
            playerController.SideInput = 0;
        }
    }

    public void OnGrab(CallbackContext ctx)
    {
        if (ctx.started)
        {
            grabComponent.GrabObject();
        }
    }
    public void OnThrow(CallbackContext ctx)
    {
        if (ctx.started)
        {
            throwComponent.ChargeObject();
        }
        else if (ctx.canceled)
        {
            throwComponent.ThrowObject();
        }
    }

    public void OnEquipUnequip(CallbackContext ctx)
    {
        if (ctx.started)
        {
            equipableManager.ChangeEquipState();
        }
    }

    public void OnDash(CallbackContext ctx)
    {
        if (ctx.started)
        {
            playerController.ApplyRoll();
        }
    }

    /*
    private RigidbodyCharacterController charController;
    private Grab grabComponent;
    private Throw throwComponent;
    private EquipUnequipController equipUnequipComponent;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction jumpAction;

    private InputAction grabAction;
    private InputAction dropAction;

    private InputAction throwAction;

    private InputAction equipAction;

    private void Awake()
    {
        charController = GetComponent<RigidbodyCharacterController>();
        grabComponent = GetComponent<Grab>();
        throwComponent = GetComponent<Throw>();
        equipUnequipComponent = GetComponent<EquipUnequipController>();

        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        grabAction = playerInput.actions["Grab"];
        dropAction = playerInput.actions["Drop"];
        throwAction = playerInput.actions["Throw"];
        equipAction = playerInput.actions["Equip"];

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
        bool equip = equipAction.IsPressed();

        if (grab) grabComponent.GrabObject();
        else if (drop) grabComponent.DropObject();

        if (equip) equipUnequipComponent.ChangeEquipState();
    }

    private void OnThrowStarted(InputAction.CallbackContext ctx)
    {
        throwComponent.ChargeObject();
    }

    private void OnThrowReleased(InputAction.CallbackContext ctx)
    {
        throwComponent.ThrowObject();
    }
    */
}
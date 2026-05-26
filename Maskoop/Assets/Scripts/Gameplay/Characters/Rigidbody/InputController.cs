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
            playerController.ApplyDash();
        }
    }

    public void OnStop(CallbackContext ctx)
    {
        if (ctx.started)
        {
            playerController.StopMovement();
        }
        else if (ctx.canceled)
        {
            playerController.ResumeMovement();
        }
    }

    public void OnCancelThrow(CallbackContext ctx)
    {
        if (ctx.started)
        {
            throwComponent.CancelThrow();
        }
    }


}
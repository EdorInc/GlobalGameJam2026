using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class MyPlayerInput : MonoBehaviour
{
    [Header("References")]
    private PlayerController playerController;
    private GrabAction grabComponent;
    private EquipAction equipableManager;
    private RegisterController registerController;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        grabComponent = GetComponent<GrabAction>();
        equipableManager = GetComponent<EquipAction>();
        registerController = GetComponent<RegisterController>();
    }

    public void OnMove(CallbackContext ctx)
    {
        if (ctx.performed)
        {
            playerController.OnMove(ctx.ReadValue<Vector2>());
            VibrationManager.Instance.RumblePulse(registerController.GetPlayerGamepad(), 0.1f, 0.1f, 0.1f);
        }
        else if (ctx.canceled)
        {
            playerController.OnMove(Vector2.zero);
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
            grabComponent.StartCharge();
        }
        else if (ctx.canceled)
        {
            grabComponent.ThrowObject();
        }
    }

    public void OnEquipUnequip(CallbackContext ctx)
    {
        if (ctx.started)
        {
            equipableManager.ChangeState();
        }

    }
}

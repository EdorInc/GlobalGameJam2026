using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInput : MonoBehaviour
{
    [Header("References")]
    private PlayerController playerController;
    private Grab grabComponent;
    private EquipableManager equipableManager;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        grabComponent = GetComponent<Grab>();
        equipableManager = GetComponent<EquipableManager>();
    }

    public void OnMove(CallbackContext ctx)
    {
        if (ctx.performed)
        {
            playerController.OnMove(ctx.ReadValue<Vector2>());
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

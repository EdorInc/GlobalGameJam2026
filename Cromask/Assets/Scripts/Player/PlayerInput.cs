using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInput : MonoBehaviour
{
    public void OnMove(CallbackContext ctx)
    {
        if (ctx.started) { }
        // playerMovement.OnMove(ctx.ReadValue<Vector2>());
        else if (ctx.canceled) { }
            // playerMovement.OnMove(Vector2.zero);
    }

    public void OnGrab(CallbackContext ctx)
    {
        if (ctx.started) { }
        // playerInteraction.OnGrab();
        else if (ctx.canceled) { }
            // playerInteraction.OnRelease();
    }
}

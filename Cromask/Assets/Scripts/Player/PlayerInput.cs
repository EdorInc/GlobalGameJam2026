using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    private void Awake()
    {
        // Si no asignas el PlayerController desde el Inspector, lo busca automáticamente
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
    }

    public void OnMove(CallbackContext ctx)
    {
        if (ctx.started) 
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
        if (ctx.started) { }
        // playerInteraction.OnGrab();
        else if (ctx.canceled) { }
            // playerInteraction.OnRelease();
    }
}

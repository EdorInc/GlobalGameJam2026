using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class EnemyInputController : MonoBehaviour
{
    [Header("References")]
    private CharacterMovementController playerController;
    private Grab grabComponent;
    private Throw throwComponent;

    private void Awake()
    {
        playerController = GetComponent<CharacterMovementController>();
        grabComponent = GetComponent<Grab>();
        throwComponent = GetComponent<Throw>();
    }

    public void OnMove(Vector2 movement)
    {
        Vector2 moveDir = movement;

        playerController.ForwardInput = moveDir.x;
        playerController.SideInput = moveDir.y;
    }

    public void OnGrab()
    {
        grabComponent.GrabObject();
    }
    public void OnThrow(bool start)
    {
        if (start)
        {
            throwComponent.ChargeObject();
        }
        else
        {
            throwComponent.ThrowObject();
        }
    }
}

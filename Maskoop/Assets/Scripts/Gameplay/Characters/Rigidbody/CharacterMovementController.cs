using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Move speed.")]
    public float moveSpeed = 5f;
    [Tooltip("Turn speed.")]
    public float turnSpeed = 10f;
    [Tooltip("Multiplier for horizontal control while in the air.")]
    public float airControlMultiplier = 0.5f;


    [Header("Dash Settings")]
    [Tooltip("Foward speed to apply when rolling.")]
    public float rollSpeed = 400f;
    [Tooltip("Time of the roll.")]
    public float rollTime = 0.3f;

    [HideInInspector]
    public bool isRolling = false;
    private float currentRollTime = 0;

    [Header("Throwing Settings")]
    [Tooltip("Move speed when throwing.")]
    public float throwingMoveSpeed = 3f;
    [Tooltip("Turn speed when throwing.")]
    public float throwingTurnSpeed = 6f;

    private GroundDetector groundDetector;

    private CharacterStateController characterState;
    private GameObject currentPlatform => groundDetector.MovingPlatform;

    private bool IsCharging => characterState.IsChargingThrow;
    private bool IsGrounded => groundDetector.IsGrounded;
    public bool CanMove => characterState.CanMove();

    public float ForwardInput { get; set; }
    public float SideInput { get; set; }

    new private Rigidbody rigidbody;

    private void Start()
    {
        characterState = GetComponent<CharacterStateController>();
        rigidbody = GetComponent<Rigidbody>();
        groundDetector = GetComponent<GroundDetector>();
    }

    void RotateTowardsMovementDirection(Vector3 movementVector, float turnSpeedMultiplier = 1.0f)
    {
        if (movementVector.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementVector, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                ((!IsCharging) ? turnSpeed : throwingTurnSpeed) * turnSpeedMultiplier * Time.deltaTime
            );
        }
    }

    public void ApplyRoll()
    {
        isRolling = true;
    }

    /// <summary>
    /// Processes input actions and converts them into movement
    /// </summary>
    private void FixedUpdate()
    {

        if (!CanMove)
        {
            return;
        }

        Vector3 horizontalInput = new Vector3(ForwardInput, 0f, SideInput).normalized;

        // Reset the velocity
        Vector3 currentVel = rigidbody.linearVelocity;
        currentVel.x = 0;
        currentVel.z = 0;
        rigidbody.linearVelocity = currentVel;

        if (currentPlatform != null)
        {
            transform.parent.SetParent(currentPlatform.transform);
        }
        else
        {
            transform.parent.SetParent(null);
        }

        if (isRolling)
        {
            Vector3 dashDirection = transform.forward;
            dashDirection.y = 0;

            rigidbody.linearVelocity += dashDirection * rollSpeed * Time.deltaTime;
            currentRollTime += Time.deltaTime;

            if (currentRollTime > rollTime)
            {
                currentRollTime = 0;
                isRolling = false;
            }

        }

        if (IsGrounded)
        {   
            if(!isRolling)
            {
                // Apply a forward or backward velocity based on player input
                Vector3 movementVector = horizontalInput * ((!IsCharging) ? moveSpeed : throwingMoveSpeed);

                Vector3 finalVelocity = movementVector;
               
                rigidbody.linearVelocity = finalVelocity;

                RotateTowardsMovementDirection(movementVector, 1.0f);
            }

                
        }
        else
        {
            if (!Mathf.Approximately(ForwardInput, 0f) || !Mathf.Approximately(SideInput, 0f))
            {
                Vector3 airMovementVector = horizontalInput * (moveSpeed * airControlMultiplier);
                
                RotateTowardsMovementDirection(airMovementVector, airControlMultiplier);

                // Preserve vertical velocity while in the air
                airMovementVector.y = rigidbody.linearVelocity.y; 
                rigidbody.linearVelocity = airMovementVector;
            }
        }
    }
}

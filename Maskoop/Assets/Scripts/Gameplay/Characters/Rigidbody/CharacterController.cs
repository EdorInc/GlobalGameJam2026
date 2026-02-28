using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Move speed.")]
    public float moveSpeed = 5f;
    [Tooltip("Turn speed.")]
    public float turnSpeed = 10f;

    [Header("Jump Settings")]
    [Tooltip("Whether the character can jump")]
    public bool allowJump = false;
    [Tooltip("Upward speed to apply when jumping.")]
    public float jumpSpeed = 3f;
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

    private Throw throwComponent;

    private bool IsGrounded => groundDetector.IsGrounded;

    public float ForwardInput { get; set; }
    public float SideInput { get; set; }
    public bool JumpInput { get; set; }

    new private Rigidbody rigidbody;

    private void Start()
    {
        throwComponent = GetComponent<Throw>();
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
                ((!throwComponent.charging) ? turnSpeed : throwingTurnSpeed) * turnSpeedMultiplier * Time.deltaTime
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
        Vector3 horizontalInput = new Vector3(ForwardInput, 0f, SideInput).normalized;

        if (IsGrounded)
        {
            // Reset the velocity
            rigidbody.linearVelocity = Vector3.zero;

            // Check if trying to jump
            if (JumpInput && allowJump)
                rigidbody.linearVelocity += Vector3.up * jumpSpeed;

            if (isRolling)
            {
                rigidbody.linearVelocity += transform.forward * rollSpeed * Time.deltaTime;

                currentRollTime += Time.deltaTime;

                if(currentRollTime > rollTime)
                {
                    currentRollTime = 0;
                    isRolling = false;
                }

            }
            else
            {
                // Apply a forward or backward velocity based on player input
                Vector3 movementVector = horizontalInput * ((!throwComponent.charging) ? moveSpeed : throwingMoveSpeed);

                rigidbody.linearVelocity += movementVector;

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

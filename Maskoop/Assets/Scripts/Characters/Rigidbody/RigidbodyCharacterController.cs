using UnityEngine;
using UnityEngine.InputSystem;

public class RigidbodyCharacterController : MonoBehaviour
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

    private GroundDetector groundDetector;

    private bool IsGrounded => groundDetector.IsGrounded;

    public float ForwardInput { get; set; }
    public float SideInput { get; set; }
    public bool JumpInput { get; set; }

    new private Rigidbody rigidbody;

    private void Start()
    {
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
                turnSpeed * turnSpeedMultiplier * Time.deltaTime
            );
        }
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

            // Apply a forward or backward velocity based on player input
            Vector3 movementVector = horizontalInput * moveSpeed;

            rigidbody.linearVelocity += movementVector;

            RotateTowardsMovementDirection(movementVector, 1.0f);
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

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
[DefaultExecutionOrder(-300)]
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

    [Header("Hit settings")]
    [Tooltip("Magnitud of force added to the player when getting hit")]
    [SerializeField] private float hitKnockBackForce = 20;
    [Tooltip("Magnitud of upwards force added to the player when getting hit")]
    [SerializeField] private float hitKnockBackUpwardsForce = 10;
    [Tooltip("Time the player wont be able to move after getting hit")]
    [SerializeField] private float hitStunTime = 1;

    [Header("Being grabbed movement")]
    [Tooltip("Time needed to be let free when grabbed")]
    [SerializeField] private float movementToBeFree = 1;

    [Header("On Fire")]
    [Tooltip("Time the player loses control when set on fire")]
    [SerializeField] private float onFireStunTime = 2.0f;

    [Tooltip("Time the player loses control when set on fire")]
    [SerializeField] private float onFireSpeed = 5.0f;

    private bool isOnFire = false;
    private float currentOnFireTime = 0.0f;

    private GroundDetector groundDetector;

    private CharacterStateController characterState;
    private GameObject currentPlatform => groundDetector.MovingPlatform;
    private bool wasGrounded = false;
    private bool isBeingThrown = false;
    private bool wasGrabbed = false;
    private float currentHitTime = 0;
    private bool isStunned = false;
    private float currentMovement = 0;
    private bool thrownByEnemy = false;
    private bool IsCharging => characterState.IsChargingThrow;
    private bool IsGrounded => groundDetector.IsGrounded;
    private bool CanMove => characterState.CanMove();

    private bool IsGrabbed => characterState.IsBeingGrabbed;
    private bool IsFloating => characterState.IsFloating;
    private bool IsOnFire => characterState.IsOnFire;

    public float ForwardInput { get; set; }
    public float SideInput { get; set; }

    new private Rigidbody rigidbody;

    private void Start()
    {
        characterState = GetComponent<CharacterStateController>();
        rigidbody = GetComponent<Rigidbody>();
        groundDetector = GetComponent<GroundDetector>();
    }

    private void OnEnable()
    {
        EventManager.OnDamageRecived += ReciveDamage;
        EventManager.Throw += Thrown;
    }

    private void OnDisable()
    {
        EventManager.OnDamageRecived -= ReciveDamage;
        EventManager.Throw -= Thrown;
    }

    private void Update()
    {
        if(isStunned)
        {
            currentHitTime += Time.deltaTime;
        }

        if(currentHitTime > hitStunTime)
        {
            currentHitTime = 0;
            isStunned = false;
        }

        if (isOnFire)
        {
            currentOnFireTime += Time.deltaTime;
        }

        if (currentOnFireTime > onFireStunTime)
        {
            currentOnFireTime = 0;
            isOnFire = false;
        }
    }

    void Thrown(GameObject thrownObject,bool active,GameObject thrower)
    {
        if (characterState.IsMyPlayer(thrownObject))
        {
            if (active)
            {
                if (thrower.CompareTag("Enemy"))
                {
                    thrownByEnemy = true;
                }
                else
                {
                    thrownByEnemy = false;
                }
            }
            isBeingThrown = active;
        }
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
        if (!wasGrounded && IsGrounded)
        {
            EventManager.OnFallEnded?.Invoke(gameObject);
            isBeingThrown = false;
        }
        if (wasGrounded && !IsGrounded && !IsGrabbed)
        {
            EventManager.OnFallStarted?.Invoke(gameObject);
        }

        wasGrounded = IsGrounded;
        wasGrabbed = IsGrabbed;
        Vector3 horizontalInput = new Vector3(ForwardInput, 0f, SideInput).normalized;

        if (!CanMove || isBeingThrown || isStunned)
        {
            if (IsGrabbed)
            {
                if(horizontalInput.magnitude > 0f)
                {
                    currentMovement += horizontalInput.magnitude;
                    Debug.Log("MOVIENDO");

                    if (currentMovement > movementToBeFree)
                    {
                        EventManager.TryingToBeFree?.Invoke(gameObject);
                        currentMovement = 0;
                    }
                }
                else
                {
                    currentMovement = 0;
                }
            }
            else if (isBeingThrown && !thrownByEnemy)
            {
                if(horizontalInput.magnitude > 0f)
                {
                    EventManager.TryingToMove?.Invoke(gameObject);
                }
            }
            return;
        }

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

            rigidbody.linearVelocity = dashDirection * rollSpeed * Time.deltaTime;
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
            Vector3 airMovementVector = horizontalInput * (moveSpeed * airControlMultiplier);

            RotateTowardsMovementDirection(airMovementVector, airControlMultiplier);

            // Preserve vertical velocity while in the air
            airMovementVector.y = rigidbody.linearVelocity.y;
            rigidbody.linearVelocity = airMovementVector;
        }

        if (isOnFire)
        {
            float originalSpeed = (!IsCharging) ? moveSpeed : throwingMoveSpeed;
            Vector3 baseMovement = horizontalInput * originalSpeed;

            Vector3 forcedForward = transform.forward * onFireSpeed;
            forcedForward.y = 0f;

            Vector3 finalVelocity = baseMovement + forcedForward;

            finalVelocity.y = rigidbody.linearVelocity.y;

            rigidbody.linearVelocity = finalVelocity;

            RotateTowardsMovementDirection(finalVelocity, 1.0f);
        }

    }


    public void ReciveDamage(GameObject player, Vector3 hitPosition)
    {
        if (characterState.IsMyPlayer(player))
        {
            Vector3 knockBackDirection = (transform.position - hitPosition).normalized;

            rigidbody.AddForce(knockBackDirection * hitKnockBackForce + Vector3.up * hitKnockBackUpwardsForce, ForceMode.Impulse);
            isStunned = true;
            Debug.Log("HITED");
            currentHitTime = 0;
            characterState.ReciveDamage(hitStunTime);
        }
    }

    public void IsBurning(Collider other)
    {
        if (characterState.IsOnFire)
        {
            isOnFire = true;
            currentOnFireTime = 0.0f;
        }
    }

}

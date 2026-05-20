using System;
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
    [SerializeField] private float onFireTime = 2.0f;
    [Tooltip("Speed the player loses control when set on fire")]
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
    private bool wasFalling = false;
    private bool movementStoped = false;
    private float speedMultiplier = 1;

    private bool IsCharging => characterState.IsChargingThrow;
    private bool IsGrounded => groundDetector.IsGrounded;
    private bool CanMove => characterState.CanMove();

    private bool IsGrabbed => characterState.IsBeingGrabbed;
    private bool IsFloating => characterState.IsFloating;
    private bool IsOnFire => characterState.IsOnFire;

    private float stunCounter = 0;

    public float ForwardInput { get; set; }
    public float SideInput { get; set; }

    new private Rigidbody rigidbody;

    private Respawn respawnComponent;

    private Vector3 lastPosition = Vector3.zero;
    private float timeStillInAir = 0;

    private void Start()
    {
        respawnComponent = GetComponent<Respawn>();
        characterState = GetComponent<CharacterStateController>();
        rigidbody = GetComponent<Rigidbody>();
        groundDetector = GetComponent<GroundDetector>();
    }

    private void OnEnable()
    {
        EventManager.OnDamageRecived += ReciveDamage;
        EventManager.OnThrow += Thrown;
        EventManager.OnLitOnFire += IsBurning;
    }

    private void OnDisable()
    {
        EventManager.OnDamageRecived -= ReciveDamage;
        EventManager.OnThrow -= Thrown;
        EventManager.OnLitOnFire -= IsBurning;
    }

    internal void StopMovement()
    {
        movementStoped = true;
    }

    internal void ResumeMovement()
    {
        movementStoped = false;
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

        if (currentOnFireTime > onFireTime)
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
        if (characterState.IsMyPlayer(thrower))
        {
            ResumeMovement();
        }
    }
    void RotateTowardsMovementDirection(Vector3 movementVector, float turnSpeedMultiplier = 1.0f)
    {
        if (movementVector.sqrMagnitude > 0f)
        {
            float turnSpeedUsed = IsCharging ? throwingTurnSpeed : turnSpeed;
            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, movementVector, turnSpeedUsed * Time.deltaTime, 0f);
            Quaternion targetRotation = Quaternion.LookRotation(desiredForward);
            rigidbody.MoveRotation(targetRotation);
        }
    }

    internal void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
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
        CheckForStucked();
        if (!wasGrounded && IsGrounded)
        {
            //Debug.Log("Player " + characterState.characterId + " landed.");
            EventManager.OnFallEnded?.Invoke(gameObject);
            isBeingThrown = false;
            stunCounter = 0;
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

                    if (currentMovement > movementToBeFree)
                    {
                        EventManager.OnTryingToBeFree?.Invoke(gameObject);
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
                    EventManager.OnTryingToMove?.Invoke(gameObject);
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
                Vector3 movementVector = horizontalInput;


                RotateTowardsMovementDirection(movementVector, 1.0f);

                if (IsCharging)
                {
                    if (movementStoped)
                    {
                        movementVector *= 0;
                    }
                    else{
                        movementVector *= throwingMoveSpeed;
                    }
                }
                else
                {
                    movementVector *= moveSpeed;
                }

                Vector3 finalVelocity = movementVector;

                rigidbody.linearVelocity = finalVelocity;

                
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

        rigidbody.linearVelocity *= speedMultiplier;

    }

    public void ReciveDamage(GameObject player, Vector3 hitPosition)
    {
        if (characterState.IsMyPlayer(player) && !isStunned)
        {
            Vector3 knockBackDirection = (transform.position - hitPosition).normalized;

            rigidbody.AddForce(knockBackDirection * hitKnockBackForce + Vector3.up * hitKnockBackUpwardsForce, ForceMode.Impulse);
            isStunned = true;
            currentHitTime = 0;
            characterState.ReciveDamage(hitStunTime);

            stunCounter++;
            if (stunCounter == 3)
            {
                respawnComponent.RespawnFunction();
                rigidbody.linearVelocity = Vector3.zero;
            }
        }        
    }

    public void IsBurning(Collider player)
    {
        if (characterState.IsMyPlayer(player.gameObject) && !characterState.IsOnFire)
        {
            isOnFire = true;
            currentOnFireTime = 0.0f;
        }
    }

    private void CheckForStucked()
    {
        if (!IsGrounded && !IsGrabbed && !IsFloating)
        {
            if(Vector3.Distance( lastPosition,transform.position) < 0.05)
            {
                timeStillInAir += Time.deltaTime;
            }
            else
            {
                timeStillInAir = 0;
            }

            if(timeStillInAir > 4)
            {
                respawnComponent.RespawnFunction();
            }
        }
        lastPosition = transform.position;
    }

}

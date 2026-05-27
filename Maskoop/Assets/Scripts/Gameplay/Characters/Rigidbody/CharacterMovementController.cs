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
    
    private CharacterStateController m_characterState;

    // All state checks go through CharacterStateController
    private bool IsGrounded => m_characterState.IsGrounded;
    private bool IsGrabbed => m_characterState.IsBeingGrabbed;
    private bool IsFloating => m_characterState.IsFloating;
    private bool IsOnFire => m_characterState.IsOnFire;
    private bool IsCharging => m_characterState.IsChargingThrow;
    private bool IsRolling => m_characterState.IsRolling;
    private bool CanMove => m_characterState.CanMove();
    private GameObject CurrentPlatform => m_characterState.MovingPlatform;

    public float ForwardInput { get; set; }
    public float SideInput { get; set; }

    // private bool wasGrabbed = false;
    // private bool wasFalling = false;
    // private float stunCounter = 0;

    private Rigidbody m_rigidbody;
    private Respawn m_respawnComponent;

    private bool m_wasGrounded = false;
    private bool m_isBeingThrown = false;
    private bool m_thrownByEnemy = false;
    private bool m_isStunned = false;
    private bool m_movementStopped = false;

    private float m_currentRollTime = 0f;
    private float m_currentHitTime = 0f;
    private float m_currentOnFireTime = 0f;
    private float m_currentMovement = 0f;
    private float m_speedMultiplier = 1f;
    private float m_stunCounter = 0f;

    private Vector3 m_lastPosition = Vector3.zero;
    private float m_timeStillInAir = 0f;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_characterState = GetComponent<CharacterStateController>();
        m_respawnComponent = GetComponent<Respawn>();
    }

    private void OnEnable()
    {
        EventManager.OnDamageRecived += ApplyHit;
        EventManager.OnThrow += Thrown;
        EventManager.OnLitOnFire += ApplyBurn;
    }

    private void OnDisable()
    {
        EventManager.OnDamageRecived -= ApplyHit;
        EventManager.OnThrow -= Thrown;
        EventManager.OnLitOnFire -= ApplyBurn;
    }

    internal void StopMovement() { m_movementStopped = true; }
    internal void ResumeMovement() { m_movementStopped = false; }
    internal void SetSpeedMultiplier(float multiplier) { m_speedMultiplier = multiplier; }

    private void Update()
    {
        if (m_isStunned)
        {
            m_currentHitTime += Time.deltaTime;
        }

        if (m_currentHitTime > hitStunTime)
        {
            m_currentHitTime = 0f;
            m_isStunned = false;
        }

        if (IsOnFire)
        {
            m_currentOnFireTime += Time.deltaTime;
        }

        if (m_currentOnFireTime > onFireTime)
        {
            m_currentOnFireTime = 0f;
            m_characterState.SetOnFire(false);
        }
    }

    /// <summary>
    /// Processes input and applies movement forces.
    /// </summary>
    private void FixedUpdate()
    {
        // Publish input state so AnimationController can read it without coupling to this class
        m_characterState.SetHasMovementInput(ForwardInput != 0f || SideInput != 0f);

        HandleAirStall();

        // Detect landing — fires exactly once on the frame the player touches the ground
        if (!m_wasGrounded && IsGrounded)
        {
            Debug.Log("Land.");
            EventManager.OnFallEnded?.Invoke(gameObject);
            m_isBeingThrown = false;
            m_stunCounter = 0;
        }
        // Detect leaving the ground — skip when an enemy lifts the player
        else if (m_wasGrounded && !IsGrounded && !IsGrabbed) 
        {
            Debug.Log("Falling...");
            EventManager.OnFallStarted?.Invoke(gameObject);
        }

        m_wasGrounded = IsGrounded;

        Vector3 horizontalInput = new Vector3(ForwardInput, 0f, SideInput).normalized;

        // Limited interactions remain available when movement is restricted
        if (!CanMove || m_isBeingThrown || m_isStunned)
        {
            if (IsGrabbed)
            {
                // Rapid wiggling fills the escape meter faster than slow inputs
                if (horizontalInput.magnitude > 0f)
                {
                    m_currentMovement += horizontalInput.magnitude;

                    if (m_currentMovement > movementToBeFree)
                    {
                        EventManager.OnTryingToBeFree?.Invoke(gameObject);
                        m_currentMovement = 0f;
                    }
                }
                else
                {
                    m_currentMovement = 0f;
                }
            }
            else if (m_isBeingThrown && !m_thrownByEnemy)
            {
                if (horizontalInput.magnitude > 0f)
                {
                    EventManager.OnTryingToMove?.Invoke(gameObject);
                }
            }

            return;
        }

        // Parent to a moving platform so the player inherits its velocity for free
        if (CurrentPlatform != null)
        {
            transform.parent.SetParent(CurrentPlatform.transform);
        }
        else
        {
            transform.parent.SetParent(null);
        }

        if (IsRolling)
        {
            Vector3 dashDirection = transform.forward;
            dashDirection.y = 0f;

            m_rigidbody.linearVelocity = dashDirection * rollSpeed * Time.fixedDeltaTime;
            m_currentRollTime += Time.deltaTime;

            if (m_currentRollTime > rollTime)
            {
                m_currentRollTime = 0f;
                m_characterState.SetRolling(false);
            }
        }

        if (IsGrounded)
        {
            if (!IsRolling)
            {
                Vector3 movementVector = horizontalInput;
                Turn(movementVector, 1.0f);

                if (IsCharging)
                {
                    movementVector *= m_movementStopped ? 0f : throwingMoveSpeed;
                }
                else
                {
                    movementVector *= moveSpeed;
                }

                // Directly setting velocity is safe on the ground; would kill vertical velocity in the air
                m_rigidbody.linearVelocity = movementVector;
            }
        }
        else
        {
            // Reduced air control — player can steer but with less authority than on the ground
            Vector3 airMovementVector = horizontalInput * (moveSpeed * airControlMultiplier);
            Turn(airMovementVector, airControlMultiplier);

            // Preserve vertical velocity so jumps and gravity are unaffected
            airMovementVector.y = m_rigidbody.linearVelocity.y;
            m_rigidbody.linearVelocity = airMovementVector;
        }

        // Fire adds a forced forward push on top of normal movement to simulate loss of control
        if (IsOnFire)
        {
            float baseSpeed = IsCharging ? throwingMoveSpeed : moveSpeed;
            Vector3 baseMovement = horizontalInput * baseSpeed;

            Vector3 forcedForward = transform.forward * onFireSpeed;
            forcedForward.y = 0f;

            Vector3 finalVelocity = baseMovement + forcedForward;
            finalVelocity.y = m_rigidbody.linearVelocity.y;
            m_rigidbody.linearVelocity = finalVelocity;

            Turn(finalVelocity, 1.0f);
        }

        // Global multiplier applied last so it affects all movement modes uniformly
        m_rigidbody.linearVelocity *= m_speedMultiplier;
    }

    private void Turn(Vector3 movementVector, float turnSpeedMultiplier = 1.0f)
    {
        if (movementVector.sqrMagnitude > 0f)
        {
            float turnSpeedUsed = IsCharging ? throwingTurnSpeed : turnSpeed;
            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, movementVector, turnSpeedUsed * Time.deltaTime, 0f);
            Quaternion targetRotation = Quaternion.LookRotation(desiredForward);
            m_rigidbody.MoveRotation(targetRotation);
        }
    }

    private void Thrown(GameObject thrownObject, bool active, GameObject thrower)
    {
        if (m_characterState.IsMyPlayer(thrownObject))
        {
            if (active)
            {
                m_thrownByEnemy = thrower.CompareTag("Enemy");
            }
            m_isBeingThrown = active;
        }

        if (m_characterState.IsMyPlayer(thrower))
        {
            ResumeMovement();
        }
    }

    public void ApplyDash()
    {
        m_currentRollTime = 0f;
        m_characterState.SetRolling(true);
    }

    public void ApplyHit(GameObject player, Vector3 hitPosition)
    {
        if (!m_characterState.IsMyPlayer(player) || m_isStunned)
        {
            return;
        }

        Vector3 knockBackDirection = (transform.position - hitPosition).normalized;
        m_rigidbody.AddForce(knockBackDirection * hitKnockBackForce + Vector3.up * hitKnockBackUpwardsForce, ForceMode.Impulse);

        m_isStunned = true;
        m_currentHitTime = 0f;
        m_characterState.ReceiveDamage(hitStunTime);

        m_stunCounter++;
        if (m_stunCounter == 3)
        {
            m_respawnComponent.RespawnFunction();
            m_rigidbody.linearVelocity = Vector3.zero;
        }
    }

    public void ApplyBurn(Collider playerStep, Collision playerGrab)
    {
        if (playerStep == null)
        {
            if (m_characterState.IsMyPlayer(playerGrab.gameObject) && !m_characterState.IsOnFire)
            {
                m_characterState.SetOnFire(true);
                m_currentOnFireTime = 0.0f;
            }
        } else if (playerGrab == null)
        {
            if (m_characterState.IsMyPlayer(playerStep.gameObject) && !m_characterState.IsOnFire)
            {
                m_characterState.SetOnFire(true);
                m_currentOnFireTime = 0.0f;
            }
        }
    }

    private void HandleAirStall()
    {
        if (!IsGrounded && !IsGrabbed && !IsFloating)
        {
            if (Vector3.Distance(m_lastPosition, transform.position) < 0.05f)
            {
                m_timeStillInAir += Time.deltaTime;
            }
            else
            {
                m_timeStillInAir = 0f;
            }

            if (m_timeStillInAir > 4f)
            {
                m_respawnComponent.RespawnFunction();
            }
        }

        m_lastPosition = transform.position;
    }

}

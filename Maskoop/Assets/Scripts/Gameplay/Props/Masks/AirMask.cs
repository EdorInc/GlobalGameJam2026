using UnityEngine;
[DefaultExecutionOrder(300)]
public class AirMask : BaseMask
{
    [Header("Flutter Settings")]
    [Tooltip("Amount of resistance of the air, the bigger the less icy.")]
    [SerializeField] private float flutterDrag = 0;
    [Tooltip("Time the player remains in the flutter state.")]
    [SerializeField] private float flutterTime = 2;
    [Tooltip("Reduction of max speed while fluttering.")]
    [SerializeField] private float flutterSpeedReduction = 0.75f;

    [Header("Visual Settings")]
    [SerializeField] private GameObject windParticlesPrefab;
    private GameObject windParticlesObject;

    private Rigidbody playerRigidBody;
    private Collider m_activeCurrent;

    private bool m_isFluttering = false;
    private bool m_wasVerticalCurrent = false;

    private float currentFloatTime = 0;

    private Vector3 lastSpeed = Vector3.zero;
    private Vector3 currentSpeed = Vector3.zero;

    public override void OnUnequip()
    {
        EventManager.OnFallStarted -= StartFlutter;
        EventManager.OnFallEnded -= EndFlutter;
        EventManager.OnAirCurrentEnter -= AirCurrentEnter;
        EventManager.OnAirCurrentExit -= AirCurrentExit;
        EventManager.OnTryingToMove -= StartFlutter;

        StopFlutter();

        Destroy(windParticlesObject);

        base.OnUnequip();
    }

    public override void OnEquip(CharacterStateController characterState)
    {
        base.OnEquip(characterState);

        EventManager.OnFallStarted += StartFlutter;
        EventManager.OnFallEnded += EndFlutter;
        EventManager.OnAirCurrentEnter += AirCurrentEnter;
        EventManager.OnAirCurrentExit += AirCurrentExit;
        EventManager.OnTryingToMove += StartFlutter;
    }

    public override void UpdateLogic()
    {

    }

    public override void FixedUpdateLogic()
    {
        if (playerRigidBody == null)
        {
            return;
        }

        // Compute world-space decomposition of the current if present
        const float kVerticalThreshold = 0.01f;
        float verticalAlongWorld = 0f;

        Vector3 vertical = Vector3.zero;
        Vector3 horizontal = Vector3.zero;

        if (currentSpeed != Vector3.zero)
        {
            verticalAlongWorld = Vector3.Dot(currentSpeed, Vector3.up);

            vertical = Vector3.Project(currentSpeed, Vector3.up);
            horizontal = currentSpeed - Vector3.up * verticalAlongWorld;
        }

        bool hasHorizontalCurrent = horizontal.sqrMagnitude > 0f;
        bool hasVerticalCurrent = Mathf.Abs(verticalAlongWorld) > kVerticalThreshold;

        if (m_isFluttering)
        {
            // When an air current is present preserve the flutter.
            if (hasHorizontalCurrent || hasVerticalCurrent)
            {
                Vector3 targetSpeed = playerRigidBody.linearVelocity * flutterSpeedReduction;
                lastSpeed = Vector3.MoveTowards(lastSpeed, targetSpeed, Time.deltaTime * flutterDrag);
            }
            else
            {
                Vector3 targetSpeed = playerRigidBody.linearVelocity * flutterSpeedReduction;
                targetSpeed = Vector3.MoveTowards(lastSpeed, targetSpeed, Time.deltaTime * flutterDrag);
                playerRigidBody.linearVelocity = targetSpeed;
                lastSpeed = playerRigidBody.linearVelocity;
                currentFloatTime += Time.deltaTime;
            }
        }

        if (currentSpeed != Vector3.zero)
        {
            if (hasHorizontalCurrent)
            {
                playerRigidBody.AddForce(horizontal);
            }

            if (hasVerticalCurrent)
            {
                playerRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
                playerRigidBody.AddForce(vertical);
            }
        }

        if (currentFloatTime > flutterTime)
        {
            StopFlutter();
        }
    }

    private void ShowFlutterVisuals(GameObject target)
    {
        if (windParticlesObject != null)
        {
            windParticlesObject.SetActive(true);
        }
        else
        {
            Collider collider = target.GetComponent<Collider>();
            Vector3 feetPosition = new Vector3(
                target.transform.position.x,
                collider.bounds.min.y,
                target.transform.position.z
            );
            windParticlesObject = Instantiate(
                windParticlesPrefab,
                feetPosition,
                Quaternion.Euler(90f, 0f, 0f),
                target.transform
            );
        }
    }

    public void StartFlutter(GameObject target)
    {
        if (!characterState.IsMyPlayer(target) || characterState.IsGrounded)
            return;

        m_isFluttering = true;
        characterState.IsFloating = true;
        playerRigidBody = target.GetComponent<Rigidbody>();
        playerRigidBody.constraints = RigidbodyConstraints.FreezePositionY;
        lastSpeed = playerRigidBody.linearVelocity;

        ShowFlutterVisuals(target);
    }

    public void EndFlutter(GameObject target)
    {
        if (characterState.IsMyPlayer(target))
        {
            StopFlutter();
            playerRigidBody = null;
        }
    }

    private void StopFlutter()
    {
        m_isFluttering = false;
        currentFloatTime = 0;

        if(playerRigidBody != null)
        {
            playerRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        characterState.IsFloating = false;
        lastSpeed = Vector3.zero;

        if (windParticlesObject != null)
        {
            windParticlesObject.SetActive(false);
        }
    }
    public void AirCurrentEnter(Collider collider, Collider current, Vector3 force)
    {
        if (characterState.IsMyPlayer(collider.gameObject))
        {
            playerRigidBody = collider.attachedRigidbody;

            m_activeCurrent = current;
            currentSpeed = force;

            StartFlutter(collider.gameObject);
        }
    }

    public void AirCurrentExit(Collider collider, Collider current)
    {
        if (characterState.IsMyPlayer(collider.gameObject))
        {
            if (current == m_activeCurrent)
            {
                m_activeCurrent = null;
                currentSpeed = Vector3.zero;
            }

            // if (m_wasVerticalCurrent && playerRigidBody != null)
            // {
            //     playerRigidBody.linearVelocity = new Vector3(
            //         playerRigidBody.linearVelocity.x,
            //         0f,
            //         playerRigidBody.linearVelocity.z
            //     );
            // }

            StartFlutter(collider.gameObject);
            m_wasVerticalCurrent = false;
        }
    }
}

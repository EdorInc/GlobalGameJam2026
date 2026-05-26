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

    private bool isFluttering = false;

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

        Vector3 horizontal = Vector3.zero;

        if (currentSpeed != Vector3.zero)
        {
            verticalAlongWorld = Vector3.Dot(currentSpeed, Vector3.up);
            horizontal = currentSpeed - Vector3.up * verticalAlongWorld;
        }

        bool hasHorizontalCurrent = horizontal.sqrMagnitude > 0f;
        bool hasVerticalCurrent = Mathf.Abs(verticalAlongWorld) > kVerticalThreshold;

        if (isFluttering)
        {
            // When an air current is present and purely horizontal preserve the flutter.
            if (!(hasHorizontalCurrent && !hasVerticalCurrent))
            {
                // Modify the player speed to create a fluttering effect, reducing speed and adding drag.
                Vector3 targetSpeed = playerRigidBody.linearVelocity * flutterSpeedReduction;
                targetSpeed = Vector3.MoveTowards(lastSpeed, targetSpeed, Time.deltaTime * flutterDrag);
                playerRigidBody.linearVelocity = targetSpeed;

                lastSpeed = playerRigidBody.linearVelocity;

                // Count the time spent fluttering.
                currentFloatTime += Time.deltaTime;
            }
            else
            {
                // Still update lastSpeed to the reduced velocity so transitions remain smooth.
                Vector3 targetSpeed = playerRigidBody.linearVelocity * flutterSpeedReduction;
                lastSpeed = Vector3.MoveTowards(lastSpeed, targetSpeed, Time.deltaTime * flutterDrag);
            }
        }

        if (currentSpeed != Vector3.zero)
        {
            // Apply horizontal push without ending flutter so the player does not start falling.
            if (hasHorizontalCurrent)
            {
                playerRigidBody.AddForce(horizontal);
            }

            // Only apply vertical component (and stop flutter) if it is significant.
            if (hasVerticalCurrent)
            {
                playerRigidBody.AddForce(Vector3.up * verticalAlongWorld);

                // If there is a meaningful vertical influence, stop flutter so normal vertical physics resumes.
                if (isFluttering)
                {
                    StopFlutter();
                }
            }
        }

        if (currentFloatTime > flutterTime)
        {
            StopFlutter();
        }
    }


    public void StartFlutter(GameObject target)
    {
        if (characterState.IsMyPlayer(target))
        {
            EventManager.OnThrow?.Invoke(target, false,gameObject);

            isFluttering = true;
            characterState.IsFloating = true;

            playerRigidBody = target.GetComponent<Rigidbody>();
            playerRigidBody.constraints = RigidbodyConstraints.FreezePositionY;
            
            lastSpeed = playerRigidBody.linearVelocity;

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

                Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);
                windParticlesObject = Instantiate(
                    windParticlesPrefab,
                    feetPosition,
                    rotation,
                    target.transform
                );
            }
        }
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
        isFluttering = false;
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

    public void AirCurrentEnter(Collider collider, Vector3 force)
    {
        if (characterState.IsMyPlayer(collider.gameObject))
        {
            // if (isFluttering)
            // {
            //     StopFlutter();
            // }

            playerRigidBody = collider.attachedRigidbody;
            currentSpeed = force;
        } 
    }

    public void AirCurrentExit(Collider collider)
    {
        if (characterState.IsMyPlayer(collider.gameObject))
        {
            currentSpeed = Vector3.zero;
            StartFlutter(collider.gameObject);
        }
    }
}

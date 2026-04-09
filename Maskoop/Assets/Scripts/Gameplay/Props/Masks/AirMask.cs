using UnityEngine;
[DefaultExecutionOrder(300)]
public class AirMask : BaseMask
{
    [Header("Flutter Settings")]
    [Tooltip("Amount of resistance of the air (The bigger the less icy)")]
    [SerializeField] private float flutterDrag = 0;
    [Tooltip("Time the player remains in the flutter state")]
    [SerializeField] private float flutterTime = 2;
    [Tooltip("Reduction of max speed while fluttering")]
    [SerializeField] private float flutterSpeedReduction = 0.75f;

    [Header("Visual Settings")]
    [SerializeField] private GameObject windParticlesPrefab;

    private bool IsFluttering = false;
    private Rigidbody playerRigidBody;

    private float currentFloatTime = 0;
    private Vector3 lastSpeed = Vector3.zero;

    private GameObject windParticlesObject;
    private Vector3 currentSpeed = Vector3.zero;

    private bool isVerticalCurrent = false;

    public override void OnUnequip()
    {
        EventManager.OnFallStarted -= StartFlutter;
        EventManager.OnFallEnded -= EndFlutter;
        EventManager.OnAirCurrentEnter -= AirCurrentEnter;
        EventManager.OnAirCurrentExit -= AirCurrentExit;
        EventManager.OnTryingToMove -= StartFlutter;
        Destroy(windParticlesObject);
        base.OnUnequip();
    }

    public override void UpdateLogic()
    {
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

    public void StartFlutter(GameObject target)
    {
        if (characterState.IsMyPlayer(target))
        {
            EventManager.OnThrow?.Invoke(target, false,gameObject);
            playerRigidBody = target.GetComponent<Rigidbody>();
            IsFluttering = true;
            playerRigidBody.constraints = RigidbodyConstraints.FreezePositionY;
            characterState.IsFloating = true;
            lastSpeed = playerRigidBody.linearVelocity;

            if(windParticlesObject != null)
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
            ResetFlutter();
            playerRigidBody = null;
        }
    }

    private void ResetFlutter()
    {
        IsFluttering = false;
        currentFloatTime = 0;
        if(playerRigidBody != null)
        {
            playerRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        characterState.IsFloating = false;
        lastSpeed = Vector3.zero;
        windParticlesObject.SetActive(false);
    }

    public override void FixedUpdateLogic()
    {
        if (IsFluttering)
        {
            Vector3 targetSpeed = playerRigidBody.linearVelocity * flutterSpeedReduction;

            targetSpeed = Vector3.MoveTowards(lastSpeed, targetSpeed, Time.deltaTime * flutterDrag);

            playerRigidBody.linearVelocity = targetSpeed;
            currentFloatTime += Time.deltaTime;
            lastSpeed = playerRigidBody.linearVelocity;
        }
        if(currentSpeed != Vector3.zero)
        {
            playerRigidBody?.AddForce(currentSpeed);
            if (IsFluttering && isVerticalCurrent)
            {
                ResetFlutter();
            }
        }
        if (currentFloatTime > flutterTime)
        {
            ResetFlutter();
        }

    }

    public void AirCurrentEnter(Collider collider, Vector3 force, bool isVertical)
    {
        isVerticalCurrent = isVertical;
        if (characterState.IsMyPlayer(collider.gameObject))
        {
            if (IsFluttering && isVertical)
            {
                ResetFlutter();
            }
            playerRigidBody = collider.attachedRigidbody;
            currentSpeed = force;
            Debug.Log("ENTRADO");
        } 
    }

    public void AirCurrentExit(Collider collider, bool isVertical)
    {
        if (characterState.IsMyPlayer(collider.gameObject))
        {
            currentSpeed = Vector3.zero;
            StartFlutter(collider.gameObject);
            Debug.Log("SALIDO");
        }
    }
}

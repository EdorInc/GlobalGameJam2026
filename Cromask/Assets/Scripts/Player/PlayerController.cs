using FMOD;
using FMODUnity;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -6.81f;

    private CharacterController characterController;
    private Vector2 moveDirection;
    private Vector3 platformVelocity = Vector3.zero;
    private Vector3 verticalVelocity;
    private bool attached = false;

    private bool isPlayingFootsteps = false;
    private bool wasMovingLastFrame = false;

    [SerializeField] private bool useBlueMask = false;

    private MaskManager maskManager;
    private Mask lastEquipedMask;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }
    }

    private void Start()
    {
        maskManager = GetComponent<MaskManager>();
        lastEquipedMask = maskManager.GetCurrentMask();
    }

    private void Update()
    {
        HandleMask();
        HandleMovement();
        HandleFootsteps();
    }

    private void HandleMask()
    {
        Mask currentMask = maskManager.GetCurrentMask();

        if (currentMask != lastEquipedMask)
        {
            lastEquipedMask = currentMask;
            useBlueMask = currentMask == Mask.Blue;
        }

        if (useBlueMask)
        {
            CheckPlatform();
        }
    }

    private void HandleMovement()
    {
        Vector3 horizontalMove = new Vector3(moveDirection.x, 0f, moveDirection.y) * moveSpeed;
        Vector3 move = horizontalMove + platformVelocity;

        if (attached)
        {
            verticalVelocity.y = platformVelocity.y > 0 ? platformVelocity.y : -2f;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        move += verticalVelocity;
        characterController.Move(move * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(
                new Vector3(moveDirection.x, 0f, moveDirection.y)
            );
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void HandleFootsteps()
    {
        bool isMovingNow = moveDirection.sqrMagnitude > 0.01f;

        // START footsteps
        if (isMovingNow && !wasMovingLastFrame)
        {
            StartFootsteps();
        }

        // STOP footsteps
        if (!isMovingNow && wasMovingLastFrame)
        {
            StopFootsteps();
        }

        wasMovingLastFrame = isMovingNow;
    }

    public void OnMove(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    private void StartFootsteps()
    {
        if (isPlayingFootsteps) return;

        isPlayingFootsteps = true;

        ATTRIBUTES_3D attr = new ATTRIBUTES_3D
        {
            position = RuntimeUtils.ToFMODVector(transform.position),
            forward = RuntimeUtils.ToFMODVector(transform.forward),
            up = RuntimeUtils.ToFMODVector(transform.up)
        };

        if (gameObject == ReferenceManager.Instance.GetPlayerOne())
        {
            AudioManager.Instance.PlayFootstep(AudioType.Footstep, attr);
        }
        else if (gameObject == ReferenceManager.Instance.GetPlayerTwo())
        {
            AudioManager.Instance.PlayFootstep2(AudioType.Footstep, attr);
        }
    }

    private void StopFootsteps()
    {
        if (!isPlayingFootsteps) return;

        isPlayingFootsteps = false;

        if (gameObject == ReferenceManager.Instance.GetPlayerOne())
        {
            AudioManager.Instance.StopFootstep();
        }
        else if (gameObject == ReferenceManager.Instance.GetPlayerTwo())
        {
            AudioManager.Instance.StopFootstep2();
        }
    }

    private void CheckPlatform()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, characterController.height / 2f + 0.2f))
        {
            if (hit.collider.CompareTag("MovingPlatform"))
            {
                MovingObject platform = hit.collider.GetComponent<MovingObject>();
                if (platform != null)
                {
                    platformVelocity = platform.GetVelocity();
                    attached = true;
                    return;
                }
            }
        }

        platformVelocity = Vector3.zero;
        attached = false;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetRotationSpeed()
    {
        return rotationSpeed;
    }
}

using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;

    private int isMovingHash;
    private int isJumpingHash;
    private int isGroundedHash;
    private int moveSpeedHash;
    private int isFallingHash;
    private int isRollingHash;
    private int cantPerformActionHash;
    private int isGrabbedHash;
    private int hasInputHash;
    private int isFloatingHash;

    private string isMovingParameter = "IsMoving";
    private string isJumpingParameter = "IsJumping";
    private string isGroundedParameter = "IsGrounded";
    private string isFalingParameter = "IsFalling";
    private string moveSpeedParameter = "MovingAnimationSpeed";
    private string isRollingParameter = "IsRolling";
    private string cantPerformActionParameter = "CantPerformAction";
    private string isGrabbedParameter = "IsGrabbed";
    private string hasInputParameter = "HasInput";
    private string isFloatingParameter = "IsFloating";

    private GroundDetector groundDetector;
    private CharacterMovementController characterMovementController;
    private CharacterStateController characterState;

    private bool IsGrounded => groundDetector.IsGrounded;
    private bool IsRolling => characterMovementController.isRolling;
    private bool IsGrabbed => characterState.IsBeingGrabbed;
    private bool IsCharging => characterState.IsChargingThrow;

    private bool IsFloating => characterState.IsFloating;

    new private Rigidbody rigidbody;

    private void OnEnable()
    {
        EventManager.OnCantPerforAction += SetCantPerformAction;
    }
    private void OnDisable()
    {
        EventManager.OnCantPerforAction -= SetCantPerformAction;
    }

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        groundDetector = GetComponent<GroundDetector>();
        animator = GetComponent<Animator>();
        characterMovementController = GetComponent<CharacterMovementController>();
        characterState = GetComponent<CharacterStateController>();


        isMovingHash = Animator.StringToHash(isMovingParameter);
        isJumpingHash = Animator.StringToHash(isJumpingParameter);
        isGroundedHash = Animator.StringToHash(isGroundedParameter);
        moveSpeedHash = Animator.StringToHash(moveSpeedParameter);
        isFallingHash = Animator.StringToHash(isFalingParameter);
        isRollingHash = Animator.StringToHash(isRollingParameter);
        cantPerformActionHash = Animator.StringToHash(cantPerformActionParameter);
        hasInputHash = Animator.StringToHash(hasInputParameter);
        isGrabbedHash = Animator.StringToHash(isGrabbedParameter);
        isFloatingHash = Animator.StringToHash(isFloatingParameter);

        if (animator == null)
        {
            Debug.Log("Animator component not found on " + gameObject.name + ", trying on children...");
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("Animator component not found in children of " + gameObject.name);
        }
        else
        {
            Debug.Log("Animator controller found.");
        }
    }

    void Update()
    {
        bool RigidbodyJumping = rigidbody.linearVelocity.y > 0f;
        bool RigidbodyMoving = new Vector3(rigidbody.linearVelocity.x, 0f, rigidbody.linearVelocity.z).sqrMagnitude > 0.01f;
        bool RigidbodyInput = characterMovementController.SideInput != 0 || characterMovementController.ForwardInput != 0;


        animator.SetBool(isFloatingHash, IsFloating);
        if (IsGrounded)
        {
            animator.SetBool(isGroundedHash, true);
            animator.SetBool(isJumpingHash, false);
            animator.SetBool(isFallingHash, false);
            animator.SetBool(isRollingHash, IsRolling);
            animator.SetBool(isMovingHash, RigidbodyMoving);   
        }
        else
        {
            animator.SetBool(isGrabbedHash, IsGrabbed);
            animator.SetBool(isGroundedHash, false);
            animator.SetBool(isJumpingHash, RigidbodyJumping);
            animator.SetBool(isFallingHash, !RigidbodyJumping);
            animator.SetBool(hasInputHash,RigidbodyInput);
            animator.SetBool(isMovingHash, RigidbodyMoving);
        }
    }       


    private void SetCantPerformAction(GameObject sender)
    {
        CharacterStateController player = sender.GetComponent<CharacterStateController>();
        if(player != null)
        {
            if(player.characterId == GetComponent<CharacterStateController>().characterId)
            {
                animator.SetTrigger(cantPerformActionHash);
            }
        }
    }
}

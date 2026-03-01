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

    private string isMovingParameter = "IsMoving";
    private string isJumpingParameter = "IsJumping";
    private string isGroundedParameter = "IsGrounded";
    private string isFalingParameter = "IsFalling";
    private string moveSpeedParameter = "MovingAnimationSpeed";
    private string isRollingParameter = "IsRolling";
    private string cantPerformActionParameter = "CantPerformAction";

    private GroundDetector groundDetector;
    private CharacterController characterController;

    private bool IsGrounded => groundDetector.IsGrounded;
    private bool IsRolling => characterController.isRolling;

    new private Rigidbody rigidbody;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        groundDetector = GetComponent<GroundDetector>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        isMovingHash = Animator.StringToHash(isMovingParameter);
        isJumpingHash = Animator.StringToHash(isJumpingParameter);
        isGroundedHash = Animator.StringToHash(isGroundedParameter);
        moveSpeedHash = Animator.StringToHash(moveSpeedParameter);
        isFallingHash = Animator.StringToHash(isFalingParameter);
        isRollingHash = Animator.StringToHash(isRollingParameter);
        cantPerformActionHash = Animator.StringToHash(cantPerformActionParameter);


        //Set event for changing the cantPerformAction

        EventManager.OnCantPerforAction += SetCantPerformAction;

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
        bool RigidbodyGrounded = IsGrounded;
        bool RigidbodyJumping = rigidbody.linearVelocity.y > 0f;
        bool RigidbodyMoving = new Vector3(rigidbody.linearVelocity.x, 0f, rigidbody.linearVelocity.z).sqrMagnitude > 0.01f;

        if (IsGrounded)
        {
            animator.SetBool(isGroundedHash, true);
            animator.SetBool(isJumpingHash, false);
            animator.SetBool(isFallingHash, false);

            if (IsRolling)
            {
                animator.SetBool(isRollingHash, true);
            }
            else
            {
                animator.SetBool(isRollingHash, false);
            }

            if (RigidbodyMoving)
                animator.SetBool(isMovingHash, true);
            else
                animator.SetBool(isMovingHash, false);
        }
        else
        {
            animator.SetBool(isGroundedHash, false);

            if (RigidbodyJumping)
            {
                animator.SetBool(isJumpingHash, true);
                animator.SetBool(isFallingHash, false);
            }
            else
            {
                animator.SetBool(isJumpingHash, false);
                animator.SetBool(isFallingHash, true);
            }

            if (RigidbodyMoving)
                animator.SetBool(isMovingHash, true);
            else
                animator.SetBool(isMovingHash, false);
        }
    }       


    private void SetCantPerformAction(bool state)
    {
        animator.SetTrigger(cantPerformActionHash);
    }
}

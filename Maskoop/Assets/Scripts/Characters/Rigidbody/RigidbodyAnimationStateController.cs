using UnityEngine;

public class RigidbodyAnimationStateController : MonoBehaviour
{
    private Animator animator;

    private int isMovingHash;
    private int isJumpingHash;
    private int isGroundedHash;
    private int moveSpeedHash;
    private int isFallingHash;

    private string isMovingParameter = "IsMoving";
    private string isJumpingParameter = "IsJumping";
    private string isGroundedParameter = "IsGrounded";
    private string isFalingParameter = "IsFalling";
    private string moveSpeedParameter = "MovingAnimationSpeed";

    private GroundDetector groundDetector;
    private bool IsGrounded => groundDetector.IsGrounded;

    new private Rigidbody rigidbody;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        groundDetector = GetComponent<GroundDetector>();
        animator = GetComponent<Animator>();

        isMovingHash = Animator.StringToHash(isMovingParameter);
        isJumpingHash = Animator.StringToHash(isJumpingParameter);
        isGroundedHash = Animator.StringToHash(isGroundedParameter);
        moveSpeedHash = Animator.StringToHash(moveSpeedParameter);
        isFallingHash = Animator.StringToHash(isFalingParameter);

        if (animator == null)
        {
            Debug.Log("Animator component not found on " + gameObject.name + "trying on children...");
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
}

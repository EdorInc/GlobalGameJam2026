using UnityEngine;

public class AnimationController : MonoBehaviour
{
    // Animation parameter name constants — centralized to prevent typos
    private const string k_isMovingParam = "IsMoving";
    private const string k_isJumpingParam = "IsJumping";
    private const string k_isGroundedParam = "IsGrounded";
    private const string k_isFallingParam = "IsFalling";
    private const string k_isRollingParam = "IsRolling";
    private const string k_cantPerformActionParam = "CantPerformAction";
    private const string k_isGrabbedParam = "IsGrabbed";
    private const string k_hasInputParam = "HasInput";
    private const string k_isFloatingParam = "IsFloating";

    private int m_isMovingHash;
    private int m_isJumpingHash;
    private int m_isGroundedHash;
    private int m_isFallingHash;
    private int m_isRollingHash;
    private int m_cantPerformActionHash;
    private int m_isGrabbedHash;
    private int m_hasInputHash;
    private int m_isFloatingHash;

    // All state reads go through CharacterStateController
    private bool IsGrounded => m_characterState.IsGrounded;
    private bool IsRolling => m_characterState.IsRolling;
    private bool IsGrabbed => m_characterState.IsBeingGrabbed;
    private bool IsFloating => m_characterState.IsFloating;

    private Animator m_animator;
    private Rigidbody m_rigidbody;
    private CharacterStateController m_characterState;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_characterState = GetComponent<CharacterStateController>();

        if (!TryGetComponent<Animator>(out m_animator))
        {
            m_animator = GetComponentInChildren<Animator>();
            if (m_animator == null)
            {
                Debug.LogError($"Animator not found on {gameObject.name} or its children.", this);
            }
        }

        m_isMovingHash = Animator.StringToHash(k_isMovingParam);
        m_isJumpingHash = Animator.StringToHash(k_isJumpingParam);
        m_isGroundedHash = Animator.StringToHash(k_isGroundedParam);
        m_isFallingHash = Animator.StringToHash(k_isFallingParam);
        m_isRollingHash = Animator.StringToHash(k_isRollingParam);
        m_cantPerformActionHash = Animator.StringToHash(k_cantPerformActionParam);
        m_hasInputHash = Animator.StringToHash(k_hasInputParam);
        m_isGrabbedHash = Animator.StringToHash(k_isGrabbedParam);
        m_isFloatingHash = Animator.StringToHash(k_isFloatingParam);
    }

    private void OnEnable()
    {
        EventManager.OnCantPerforAction += SetCantPerformAction;
    }

    private void OnDisable()
    {
        EventManager.OnCantPerforAction -= SetCantPerformAction;
    }

    private void Update()
    {
        bool isRigidbodyJumping = m_rigidbody.linearVelocity.y > 0f;
        bool isRigidbodyMoving = new Vector3(m_rigidbody.linearVelocity.x, 0f, m_rigidbody.linearVelocity.z).sqrMagnitude > 0.01f;

        m_animator.SetBool(m_isFloatingHash, IsFloating);

        if (IsGrounded)
        {
            m_animator.SetBool(m_isGroundedHash, true);
            m_animator.SetBool(m_isJumpingHash, false);
            m_animator.SetBool(m_isFallingHash, false);
            m_animator.SetBool(m_isRollingHash, IsRolling);
            m_animator.SetBool(m_isMovingHash, isRigidbodyMoving);
            m_animator.SetBool(m_isGrabbedHash, false);
            m_animator.SetBool(m_hasInputHash, false);
        }
        else
        {
            m_animator.SetBool(m_isGroundedHash, false);
            m_animator.SetBool(m_isJumpingHash, isRigidbodyJumping);
            m_animator.SetBool(m_isFallingHash, !isRigidbodyJumping);
            m_animator.SetBool(m_isMovingHash, isRigidbodyMoving);
            m_animator.SetBool(m_isGrabbedHash, IsGrabbed);
            m_animator.SetBool(m_hasInputHash, m_characterState.HasMovementInput);
            m_animator.SetBool(m_isRollingHash, false);
        }
    }

    private void SetCantPerformAction(GameObject sender)
    {
        CharacterStateController player = sender.GetComponent<CharacterStateController>();
        if (player != null && player.CharacterId == m_characterState.CharacterId)
        {
            m_animator.SetTrigger(m_cantPerformActionHash);
        }
    }
}
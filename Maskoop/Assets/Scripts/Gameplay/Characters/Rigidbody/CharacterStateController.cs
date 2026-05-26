using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStateController : MonoBehaviour
{
    [Header("Player Info")]
    [Tooltip("Id of the player")]
    [SerializeField] private int m_characterId = -1;

    /// <summary>
    /// Numeric identifier for this character instance. Can be changed at runtime to reassign ownership.
    /// </summary>
    public int CharacterId
    {
        get => m_characterId;
        set => m_characterId = value;
    }

    [SerializeField] private Renderer m_bodyRenderer;
    [SerializeField] private Renderer m_eyesRenderer;

    internal Renderer GetBodyRenderer() => m_bodyRenderer;

    // Ground state, owned here but sourced from GroundDetector
    public bool IsGrounded => m_groundDetector.IsGrounded;
    public GameObject MovingPlatform => m_groundDetector.MovingPlatform;

    // Movement state
    public bool IsRolling { get; private set; }
    public bool HasMovementInput { get; private set; }
    public bool CanMove() => !IsBeingGrabbed;

    // Grab state
    public bool IsHoldingObject => m_heldObject != null;
    public bool IsBeingGrabbed { get; private set; }

    // Elemental state
    public bool HasMaskEquipped => m_currentMask != null;
    public bool IsFloating { get; set; }
    public bool IsOnFire { get; private set; }

    // Throw state
    public bool IsChargingThrow => m_throwComponent.charging;
    
    public Grabbable GetHeldObject() => m_heldObject;
    public BaseMask GetCurrentMask() => m_currentMask;

    private Grabbable m_heldObject;
    private BaseMask m_currentMask;
    private Throw m_throwComponent;
    private Grab m_grabComponent;
    private GroundDetector m_groundDetector;

    public void SetBeingGrabbed(bool value) { IsBeingGrabbed = value; }
    public void SetHeldObject(Grabbable grabbedObject) { m_heldObject = grabbedObject; }
    public void SetOnFire(bool value) { IsOnFire = value; }
    public void SetRolling(bool value) { IsRolling = value; }
    public void SetHasMovementInput(bool value) { HasMovementInput = value; }

    private void OnEnable()
    {
        EventManager.OnRespawn += OnRespawn;
    }

    private void OnDisable()
    {
        EventManager.OnRespawn -= OnRespawn;
    }
    private void Awake()
    {
        m_groundDetector = GetComponent<GroundDetector>();
    }

    private void Start()
    {
        m_throwComponent = GetComponent<Throw>() ?? GetComponentInChildren<Throw>() ?? GetComponentInParent<Throw>();
        if (m_throwComponent == null)
        {
            Debug.LogWarning("Throw component not found. Throwing functionality will be disabled.");
        }

        m_grabComponent = GetComponent<Grab>() ?? GetComponentInChildren<Grab>() ?? GetComponentInParent<Grab>();
        if (m_grabComponent == null)
        {
            Debug.LogWarning("Grab component not found. Grabbing functionality will be disabled.");
        }
    }

    private void Update()
    {
        m_currentMask?.UpdateLogic();
    }

    private void FixedUpdate()
    {
        m_currentMask?.FixedUpdateLogic();
    }

    private void OnRespawn(GameObject player)
    {
        // Fast-fail when not our player or nothing held
        if (!IsMyPlayer(player) || !IsHoldingObject)
        {
            return;
        }

        // Cancel any in-progress throw (if present) then drop the held object.
        m_throwComponent?.CancelThrow();
        m_grabComponent?.DropObject();
    }

    // TODO Change this when we have a different model.

    public void DisableRender()
    {
        m_bodyRenderer.enabled = false;
        m_eyesRenderer.enabled = false;
    }

    public void EnableRenderer()
    {
        m_bodyRenderer.enabled = true;
        m_eyesRenderer.enabled = true;
    }

    public void EquipMask(BaseMask mask)
    {
        m_currentMask?.OnUnequip();
        m_currentMask = mask;
        m_currentMask?.OnEquip(this);
    }

    public void UnequipMask()
    {
        m_currentMask?.OnUnequip();
        m_currentMask = null;
    }

    public bool IsMyPlayer(GameObject player)
    {
        CharacterStateController otherplayer = player.GetComponent<CharacterStateController>();

        if (otherplayer == null)
        {
            return false;
        }

        return otherplayer.CharacterId == this.CharacterId;
    }

    public void ReceiveDamage(float hitTime)
    {
        if (IsHoldingObject)
        {
            // Just in case it is called multiple times, we don't want to queue up multiple releases.
            CancelInvoke(nameof(ReleaseHeldObject));
            Invoke(nameof(ReleaseHeldObject), hitTime);
        }
    }

    /// <summary>
    /// Releases the currently held object.
    /// Cancels an in-progress throw if the player is charging a throw; otherwise drops the object.
    /// </summary>
    public void ReleaseHeldObject()
    {
        if (!IsHoldingObject)
        {
            return;
        }

        if (IsChargingThrow) 
        {
            m_throwComponent?.CancelThrow();
        }
        else
        {
            m_grabComponent?.DropObject();
        }
    }
}
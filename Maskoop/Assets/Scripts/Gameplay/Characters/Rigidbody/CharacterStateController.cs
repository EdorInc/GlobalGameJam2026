using System;
using UnityEngine;

public class CharacterStateController : MonoBehaviour
{
    [Header("Player Info")]
    [Tooltip("Id of the player")]
    public int characterId = -1;

    public bool IsHoldingObject => heldObject != null;
    public bool IsBeingGrabbed { get; private set; }
    public bool IsFloating { get; set; }

    public Renderer BodyRenderer;
    public Renderer EyesRenderer;

    public bool IsOnFire { get; set; }
    public void SetOnFire(bool value) { IsOnFire = value; }
    public bool IsChargingThrow => throwComponent.charging;
    public bool HasMaskEquipped => currentMask != null;

    private Grabbable heldObject;
    private BaseMask currentMask;
    private Throw throwComponent;
    private Grab grabComponent;

    private void OnEnable()
    {
        EventManager.OnRespawn += OnRespawn;
    }

    private void OnDisable()
    {
        EventManager.OnRespawn -= OnRespawn;
    }
    private void Start()
    {
        throwComponent = GetComponent<Throw>();
        grabComponent = GetComponent<Grab>();
    }
    private void Update()
    {
        currentMask?.UpdateLogic();
    }

    private void FixedUpdate()
    {
        currentMask?.FixedUpdateLogic();
    }

    private void OnRespawn(GameObject player)
    {
        if (IsMyPlayer(player))
        {
            if (IsHoldingObject)
            {
                if (IsChargingThrow)
                {
                    throwComponent.CancelThrow();
                }
                grabComponent.DropObject();
            }
        } 
    }
    public void DisableRender()
    {
        BodyRenderer.enabled = false;
        EyesRenderer.enabled = false;

    }

    public void EnableRenderer()
    {
        BodyRenderer.enabled = true;
        EyesRenderer.enabled = true;
    }

    public void SetHeldObject(Grabbable obj)
    {
        heldObject = obj;
    }

    public Grabbable GetHeldObject()
    {
        return heldObject;
    }

    internal Renderer GetBodyRenderer()
    {
        return BodyRenderer;
    }

    public void SetBeingGrabbed(bool value)
    {
        IsBeingGrabbed = value;
    }

    public void EquipMask(BaseMask mask)
    {
        if (currentMask != null)
        {
            currentMask.OnUnequip();
        }

        currentMask = mask;

        if (currentMask != null)
        {
            currentMask.OnEquip(this);
        }
    }

    public void UnequipMask()
    {
        if (currentMask == null) return;

        currentMask.OnUnequip();
        currentMask = null;
    }

    public BaseMask GetCurrentMask()
    {
        return currentMask;
    }

    public bool CanMove()
    {
        return !IsBeingGrabbed;
    }

    public bool IsMyPlayer(GameObject player)
    {
        CharacterStateController otherplayer = player.GetComponent<CharacterStateController>();

        if (otherplayer == null)
        {
            return false;
        }
        return otherplayer.characterId == this.characterId;
    }

    public void ReciveDamage(float hitTime)
    {
        if (IsHoldingObject)
        {
            Invoke(nameof(DelayDrop), 0);
        }
    }

    public void DelayDrop()
    {
        if (IsHoldingObject)
        {
            if (IsChargingThrow)
            {
                throwComponent.CancelThrow();
            }
            else
            {
                grabComponent.DropObject();
            }
        }
    }
}
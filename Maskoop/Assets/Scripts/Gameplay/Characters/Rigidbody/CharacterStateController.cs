using UnityEngine;

public class CharacterStateController : MonoBehaviour
{
    [Tooltip("Id of the player")]
    public int characterId = -1;


    public bool IsHoldingObject => heldObject != null;
    public bool IsBeingGrabbed { get; private set; }

    public bool IsFloating { get; set; }

    public bool IsChargingThrow => throwComponent.charging;
    public bool HasMaskEquipped => currentMask != null;

    private Grabbable heldObject;
    private BaseMask currentMask;
    private Throw throwComponent;


    private void Start()
    {
        throwComponent = GetComponent<Throw>();
    }
    private void Update()
    {
        currentMask?.UpdateLogic();
    }

    private void FixedUpdate()
    {
        currentMask?.FixedUpdateLogic();
    }

    public void SetHeldObject(Grabbable obj)
    {
        heldObject = obj;
    }

    public Grabbable GetHeldObject()
    {
        return heldObject;
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
}
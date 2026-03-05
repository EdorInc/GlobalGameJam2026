using UnityEngine;

public class CharacterStateController : MonoBehaviour
{
    [Tooltip("Id of the player")]
    [SerializeField]private int characterId = -1;


    public bool IsHoldingObject => heldObject != null;
    public bool IsBeingGrabbed { get; private set; }
    public bool HasMaskEquipped => currentMask != null;

    private Grabbable heldObject;
    private BaseMask currentMask;


    private void Update()
    {
        currentMask?.updateLogic();
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
            currentMask.OnEquip();
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
}
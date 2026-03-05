using UnityEngine;

public class Equip : MonoBehaviour
{
    [Header("Equip Settings")]
    [SerializeField] private Transform equipedPosition;

    private GameObject equipedObject;

    private Grab grabComponent;

    private CharacterStateController characterState;

    void Start()
    {
        characterState = GetComponent<CharacterStateController>();
        grabComponent = GetComponent<Grab>();
    }

    void LateUpdate()
    {
        if (equipedObject != null)
        {
            Equipable equipable = equipedObject.GetComponent<Equipable>();

            // Move smoothly to the hold position
            Vector3 targetPos = equipedPosition.position + transform.forward * equipable.equipOffset;
            targetPos += Vector3.up * equipable.equipVerticalOffset;
            Quaternion targetRot = equipedPosition.rotation * equipable.equipOffsetRotation;

            equipedObject.transform.position = targetPos;
            equipedObject.transform.rotation = targetRot;
        }
    }

    public void ChangeEquipState()
    {
        if(grabComponent == null)
        {
            return;
        }

        GameObject grabbedObject = grabComponent.grabbedObject;

        if(grabbedObject == null)
        {
            if (equipedObject == null)
            {
                //No object to equip
                EventManager.OnCantPerforAction?.Invoke(gameObject);
                return;
            }
            //If no object is grabbed but an object is equipped you should unEquip
            equipedObject.GetComponent<Equipable>().UnEquip();
            characterState.UnequipMask();
            grabComponent.grabbedObject = equipedObject;
            equipedObject = null;
            return;
        }

        Equipable objectToEquip = grabbedObject.GetComponent<Equipable>();
        BaseMask maskToEquip = grabbedObject.GetComponent<BaseMask>();


        if (objectToEquip == null)
        {
            //Object cant be equipped
            EventManager.OnCantPerforAction?.Invoke(gameObject);
            return;
        }

        if(equipedObject == null)
        {
            //If no object is equip you shoudl equip held object
            equipedObject = grabbedObject;
            grabComponent.grabbedObject = null;
            objectToEquip.Equip(equipedPosition);
            if (maskToEquip != null)
            {
                characterState.EquipMask(maskToEquip);
            }

        }
        else
        {
            //If an object is equiped and grab swap them
            GameObject changeAux = equipedObject;
            equipedObject.GetComponent<Equipable>().UnEquip();
            equipedObject = grabbedObject;
            grabComponent.grabbedObject = changeAux;
            grabComponent.DropObject();
            objectToEquip.Equip(equipedPosition);

            if (maskToEquip != null)
            {
                characterState.EquipMask(maskToEquip);
            }
        }
    }

    public bool IsMaskEquiped()
    {
        return equipedObject && equipedObject.CompareTag("Mask");
    }
}

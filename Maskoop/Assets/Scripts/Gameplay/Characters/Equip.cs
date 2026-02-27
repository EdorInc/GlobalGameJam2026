using UnityEngine;

public class Equip : MonoBehaviour
{
    [Header("Equip Settings")]
    [SerializeField] private Transform equipedPosition;

    private GameObject equipedObject;

    private Grab grabComponent;

    void Start()
    {
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
                return;
            }
            //If no object is grabbed but an object is equipped you should unEquip
            equipedObject.GetComponent<Equipable>().UnEquip();
            grabComponent.grabbedObject = equipedObject;
            equipedObject = null;
            return;
        }

        Equipable objectToEquip = grabbedObject.GetComponent<Equipable>();

        if(objectToEquip == null)
        {
            return;
        }

        if(equipedObject == null)
        {
            //If no object is equip you shoudl equip held object
            equipedObject = grabbedObject;
            grabComponent.grabbedObject = null;
            objectToEquip.Equip(equipedPosition);
        }
        else
        {
            //If an object is equiped and grab swap them
            GameObject changeAux = equipedObject;
            equipedObject.GetComponent<Equipable>().UnEquip();
            equipedObject = grabbedObject;
            grabComponent.grabbedObject = changeAux;
            objectToEquip.Equip(equipedPosition);

        }
    }
}

using UnityEngine;

public class EquipUnequipController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private GameObject equipedObject;

    private Grab grabComponent;

    void Start()
    {
        grabComponent = GetComponent<Grab>();
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
            objectToEquip.Equip();
        }
    }
}

using UnityEngine;

public class EquipableManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private GameObject equipedObject;
    public void ChangeState()
    {
        if (equipedObject != null)
        {
            UnEquip();
        }
        else
        {
            Equip();
        }
    }
    private void Equip()
    {
        Grab grabScrip = GetComponent<Grab>();
        if (grabScrip == null)
        {
            return;
        }

        equipedObject = grabScrip.GetGrabbedObject();
        
        EquipableObject equipObjectScript =  equipedObject.GetComponent<EquipableObject>();
        if (equipObjectScript == null)
        {
            return;
        }
        equipObjectScript.Equip();
    }

    private GameObject UnEquip()
    {
        equipedObject.GetComponent<EquipableObject>().UnEquip();
        equipedObject = null;
        return equipedObject;
    }
}

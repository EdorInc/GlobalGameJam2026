using UnityEngine;

public class EquipAction : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private GameObject equipedObject;
    private MaskManager maskManager;

    void Start()
    {
        maskManager = GetComponent<MaskManager>();
    }

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
        GrabAction grabScrip = GetComponent<GrabAction>();
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

        Mask equipedMask = equipObjectScript.Equip();

        maskManager.ApplyMask(equipedMask);
    }

    private GameObject UnEquip()
    {
        equipedObject.GetComponent<EquipableObject>().UnEquip();
        equipedObject = null;

        maskManager.ApplyMask(Mask.Unmasked);

        return equipedObject;
    }
}

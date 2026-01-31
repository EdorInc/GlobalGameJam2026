using UnityEngine;

public class EquipAction : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private GameObject equipedObject = null;
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

        if (equipedObject == null)
        {
            return;
        }
        EquipableObject equipObjectScript =  equipedObject.GetComponent<EquipableObject>();
        if (equipObjectScript == null)
        {
            equipedObject = null;
            return;
        }
        grabScrip.RemoveGrabbedObject();
        Mask equipedMask = equipObjectScript.Equip();

        maskManager.ApplyMask(equipedMask);
    }

    private GameObject UnEquip()
    {
        GrabAction grabScrip = GetComponent<GrabAction>();

        if (grabScrip == null)
        {
            return null;
        }

        equipedObject.GetComponent<EquipableObject>().UnEquip();
        equipedObject = null;

        maskManager.ApplyMask(Mask.Unmasked);

        grabScrip.GrabObjectFromEquip();

        return equipedObject;
    }
}

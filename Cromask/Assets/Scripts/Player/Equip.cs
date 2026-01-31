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
       
        GrabAction grab = GetComponent<GrabAction>();
        if (grab == null) return;
        if (grab.IsCharging()) return;

        GameObject grabbed = grab.GetGrabbedObject();

        if (equipedObject != null && grabbed != null)
        {
            Swap(grabbed);
            return;
        }

        if (equipedObject == null && grabbed != null)
        {
            Equip(grabbed);
            return;
        }

        if (equipedObject != null && grabbed == null)
        {
            UnEquip();
        }
    }

    private void Equip(GameObject obj)
    {
        if (!obj.TryGetComponent(out EquipableObject equipable))
            return;

        GrabAction grab = GetComponent<GrabAction>();

        grab.RemoveGrabbedObject();
        equipedObject = obj;

        Mask mask = equipable.Equip();
        maskManager.ApplyMask(mask);
    }


    private void UnEquip()
    {
        if (equipedObject == null) return;

        GrabAction grab = GetComponent<GrabAction>();
        EquipableObject equip = equipedObject.GetComponent<EquipableObject>();

        equip.UnEquip();
        maskManager.ApplyMask(Mask.Unmasked);

        GameObject mask = equipedObject;
        equipedObject = null;

        grab.ForceGrabObject(mask);
    }

    private void Swap(GameObject newMask)
    {
        GrabAction grab = GetComponent<GrabAction>();

        GameObject oldMask = equipedObject;
        EquipableObject oldEquip = oldMask.GetComponent<EquipableObject>();
        oldEquip.UnEquip();


        if (newMask.TryGetComponent(out EquipableObject newEquip))
        {
            grab.RemoveGrabbedObject();
            equipedObject = newMask;
            Mask newMaskType = newEquip.Equip();
            maskManager.ApplyMask(newMaskType);
        }
        else
        {
            equipedObject = null;
            grab.Drop();
            maskManager.ApplyMask(Mask.Unmasked);
        }

        grab.ForceGrabObject(oldMask);
    }



}

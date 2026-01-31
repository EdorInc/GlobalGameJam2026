using UnityEngine;

public class EquipableObject : MonoBehaviour
{
    [SerializeField] private Mask maskType = Mask.Unmasked;

    private GameObject particleSystem;

    [SerializeField]
    private GameObject sphereTrigger;

    [SerializeField]
    private Transform particleGenerationPosition;

    public Mask Equip()
    {
        sphereTrigger.SetActive(false);
        switch (maskType)
        {
            case Mask.Red:
                particleSystem = VFXManager.Instance.PlayPermanentVFX(VFXType.EquipRedMask, particleGenerationPosition.position);
                break;
            case Mask.Blue:
                Debug.Log("Playing blue mask VFX");
                particleSystem = VFXManager.Instance.PlayPermanentVFX(VFXType.EquipBlueMask, particleGenerationPosition.position);
                break;
            case Mask.Green:
                particleSystem = VFXManager.Instance.PlayPermanentVFX(VFXType.EquipGreenMask, particleGenerationPosition.position);
                break;
        }
       

        particleSystem.transform.parent = this.transform;

        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent <Rigidbody>().isKinematic = true;
        Debug.Log("Object equipped");
        
        return maskType;
    }

    public void UnEquip()
    {
        sphereTrigger.SetActive(true);
        if (particleSystem)
        {
            Destroy(particleSystem);
        }
       
        GetComponent<Collider>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
      
        Debug.Log("Object unequipped");
    }
}

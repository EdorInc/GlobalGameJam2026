using UnityEngine;

public class EquipableObject : MonoBehaviour
{
    [SerializeField] private Mask maskType = Mask.Unmasked;

    private GameObject particleSystem;

    [SerializeField]
    private GameObject sphereTrigger;

    [SerializeField]
    private Transform particleGenerationPosition;

    [SerializeField]
    private Transform maskRender;

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
        GetComponent <Rigidbody>().isKinematic = true;
        Debug.Log("Object equipped");
        maskRender.localScale = new Vector3(0.7f,0.7f,0.7f);
        maskRender.position -= maskRender.forward * (0.8f);
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
        GetComponent<Rigidbody>().isKinematic = false;
        maskRender.localScale = new Vector3(1, 1, 1);
        maskRender.position += maskRender.forward * (0.8f);
        Debug.Log("Object unequipped");
    }
}

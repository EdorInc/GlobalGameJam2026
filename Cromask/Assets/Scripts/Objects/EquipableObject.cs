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

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = maskRender.localScale;
    }

    public Mask Equip()
    {
        sphereTrigger.SetActive(false);
        switch (maskType)
        {
            case Mask.Red:
                particleSystem = VFXManager.Instance.PlayPermanentVFX(VFXType.EquipRedMask, particleGenerationPosition.position);
                break;
            case Mask.Blue:
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
        maskRender.position -= maskRender.forward * (0.8f);
        // Debug.Log("Position set when equipped to: " + maskRender.position);
        maskRender.localScale = originalScale * 0.7f;
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
        maskRender.localScale = originalScale;
        maskRender.position += maskRender.forward * (0.8f);
        // Debug.Log("Position set when unequipped to: " + maskRender.position);
        Debug.Log("Object unequipped");
    }
}

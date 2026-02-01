using UnityEngine;

public class EquipableObject : MonoBehaviour
{
    [SerializeField] private Mask maskType = Mask.Unmasked;

    private ParticleSystem particleSystem;

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
                particleSystem = transform.GetComponentInChildren<ParticleSystem>();
                break;
            case Mask.Blue:
                particleSystem = transform.GetComponentInChildren<ParticleSystem>();
                break;
            case Mask.Green:
                particleSystem = transform.GetComponentInChildren<ParticleSystem>();
                break;
        }

        if (particleSystem)
        {
            particleSystem.Play();
        }

        GetComponent<Collider>().enabled = false;
        GetComponent <Rigidbody>().isKinematic = true;
        Debug.Log("Object equipped");
        maskRender.position -= maskRender.forward * (0.8f);
        maskRender.localScale = originalScale * 0.7f;
        return maskType;
    }

    public void UnEquip()
    {
        sphereTrigger.SetActive(true);

        if (particleSystem)
        {
            particleSystem.Stop();
        }

        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        maskRender.localScale = originalScale;
        maskRender.position += maskRender.forward * (0.8f);
        Debug.Log("Object unequipped");
    }
}

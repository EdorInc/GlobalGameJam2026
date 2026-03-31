using UnityEngine;

public class TargetSwitch : BaseSwitch
{
    [Header("Apparience Settings")]
    [Tooltip("Provisinal material to use when the target is activated.")]
    [SerializeField] protected Material activatedMaterial;

    [Tooltip("Provisinal width to use when the target is activated.")]
    [SerializeField] protected float activatedWidth = 0.1f;

    protected Material deactivatedMaterial;

    protected float deactivatedWidth;

    private new void Awake()
    {
        base.Awake();

        deactivatedMaterial = meshRenderer.material;
        deactivatedWidth = transform.localScale.z;

        Refresh();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rock") && currentState != SwitchState.Active)
        {
            Activate();
        }
    }

    protected override void SetActive()
    {
        meshRenderer.material = activatedMaterial;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, activatedWidth);
    }

    protected override void SetInactive()
    {
        meshRenderer.material = deactivatedMaterial;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, deactivatedWidth);
    }

    protected override void SetDeactivating()
    {
        throw new System.NotImplementedException();
    }
}

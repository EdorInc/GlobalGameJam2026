using UnityEngine;

public class PressurePlate : ActivatorBase
{
    [Header("Plate Settings")]
    [SerializeField] protected float activatedWidth = 0.1f;

    [Header("Activation Settings")]
    [SerializeField] protected bool canOnlyBeActivatedByPlayer = true;

    protected float deactivatedWidth;

    private void OnCollisionEnter(Collision collision)
    {
        bool willActivate = !hasBeenActivated;
        if (canOnlyBeActivatedByPlayer)
        {
            willActivate = willActivate && collision.gameObject.CompareTag("Player");
        }

        if (willActivate)
        {
            hasBeenActivated = true;

            SetApparience();

            EventManager.OnButtonPressed?.Invoke(channel);
        }
    }

    protected override void SetApparience()
    {
        if (hasBeenActivated)
        {
            meshRenderer.material = activatedMaterial;
            transform.localScale = new Vector3(transform.localScale.x, activatedWidth, transform.localScale.z);
        }
        else
        {
            meshRenderer.material = deactivatedMaterial;
            transform.localScale = new Vector3(transform.localScale.x, deactivatedWidth, transform.localScale.z);
        }
    }

    protected override void GetApparience()
    {
        deactivatedMaterial = meshRenderer.material;
        deactivatedWidth = transform.localScale.y;
    }
}

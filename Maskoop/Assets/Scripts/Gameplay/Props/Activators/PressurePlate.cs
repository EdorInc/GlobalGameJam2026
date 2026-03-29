using UnityEngine;

public class PressurePlate : ActivatorBase
{
    [Header("Plate Settings")]
    [SerializeField] protected float activatedWidth = 0.1f;

    [Header("Activation Settings")]
    [SerializeField] protected bool canOnlyBeActivatedByPlayer = true;

    protected float deactivatedWidth;

    private void OnTriggerEnter(Collider other)
    {
        bool willActivate = !hasBeenActivated;

        if (canOnlyBeActivatedByPlayer)
        {
            willActivate = willActivate && other.gameObject.CompareTag("Player");
        }

        if (willActivate)
        {
            hasBeenActivated = true;

            SetApparience();

            Debug.Log("Activating channel " + channel + " through pressure plate.");

            EventManager.OnButtonPressed?.Invoke(channel);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        bool willActivate = !hasBeenActivated;

        if (canOnlyBeActivatedByPlayer)
        {
            willActivate = willActivate && other.gameObject.CompareTag("Player");
        }

        if (!willActivate)
        {
            hasBeenActivated = false;

            SetApparience();

            Debug.Log("Deactivating channel " + channel + " through pressure plate.");

            // TODO - Add a new event for deactivation.
            // EventManager.OnButtonPressed?.Invoke(channel);
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

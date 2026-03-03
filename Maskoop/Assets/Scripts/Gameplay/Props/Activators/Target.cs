using UnityEngine;

public class Target : ActivatorBase
{
    [Header("Target Settings")]
    [SerializeField] protected float activatedWidth = 0.1f;

    protected float deactivatedWidth;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rock") && !hasBeenActivated)
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
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, activatedWidth);
        }
        else
        {
            meshRenderer.material = deactivatedMaterial;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, deactivatedWidth);
        }
    }

    protected override void GetApparience()
    {
        deactivatedMaterial = meshRenderer.material;
        deactivatedWidth = transform.localScale.z;
    }
}

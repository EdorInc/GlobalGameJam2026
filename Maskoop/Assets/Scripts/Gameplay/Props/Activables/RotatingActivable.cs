using UnityEngine;

public class RotatingActivable : BaseActivable
{
    [Header("Movement settings")]
    [Tooltip("The speed at which the object rotates to its final position.")]
    [SerializeField] private float rotationSpeed = 5;
    [Tooltip("The final rotation of the object when the animation is complete.")]
    [SerializeField] private Quaternion finalRotation;

    protected override void ActivateAnimation()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, finalRotation, Time.deltaTime * rotationSpeed);
    }

    protected override void DeactivateAnimation()
    {
        throw new System.NotImplementedException("Deactivation animation not implemented for RotatingActivable.");
    }

    // TODO - Add the condition when it is deactivating.
    protected override bool StopCondition()
    {
        return transform.rotation.Equals(finalRotation);
    }

}

using UnityEngine;

public class Bridge : ActivableBase
{
    [Header("Movement settings")]
    [SerializeField] private float rotationSpeed = 5;
    [SerializeField] private Quaternion finalRotation;

    protected override void ActivatedAction()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, finalRotation, Time.deltaTime * rotationSpeed);
    }

    protected override bool StopCondition()
    {
        return transform.rotation.Equals(finalRotation);
    }

}

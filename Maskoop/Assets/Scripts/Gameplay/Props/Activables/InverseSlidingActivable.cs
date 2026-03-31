using UnityEngine;

public class InverseSlidingActivable : SlidingActivable
{
    [Header("Direction Settings")]
    [Tooltip("Whether the calculated end position is forward or backwards to adact to the map layout.")]
    [SerializeField] bool invertPosition = false;

    protected new void Start()
    {
        base.Start();

        Vector3 finalPosition = openPosition.position;

        if (invertPosition)
        {
            Vector3 direction = finalPosition - closedPosition;
            finalPosition = closedPosition - direction;
        }

        // Swap positions
        transform.position = finalPosition;
        openPosition.position = closedPosition;
    }
}

using UnityEngine;

public class InverseDoor : Door
{
    private Vector3 closedPosition;

    [Header("Direction Settings")]
    [SerializeField] bool invertPosition = false;

    void Start()
    {
        Vector3 closedPosition = transform.position;

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

using UnityEngine;

public class SlidingActivable : BaseActivable
{
    [Header("Position Settings")]
    [SerializeField]
    [Tooltip("Position of the game object when it opens.")]
    protected Transform openPosition;

    [Header("Movement Settings")]
    [SerializeField]
    [Tooltip("Speed to open the game object.")]
    private float speed = 2;

    protected Vector3 closedPosition;

    protected new void Start()
    {
        base.Start();
        closedPosition = transform.position;
    }

    protected override bool StopCondition()
    {
        Vector3 endPosition = openPosition.position;
        if (currentState == ActivableState.Deactivating)
        {
            endPosition = closedPosition;
        }

        // Define a small margin of error (epsilon)
        const float positionTolerance = 0.01f;
        bool isAtEndPosition = Vector3.Distance(transform.position, endPosition) < positionTolerance;

        if (isAtEndPosition)
        {
            if (currentState == ActivableState.Activating)
            {
                Debug.Log("Door " + gameObject.name + " is at open position.");
            }
            else if (currentState == ActivableState.Deactivating)
            {
                Debug.Log("Door " + gameObject.name + " is at closed position.");
            }
        }

        return isAtEndPosition;
    }

    protected override void ActivateAnimation()
    {
        Debug.Log("Moving door " + gameObject.name + " towards open position...");
        transform.position = Vector3.MoveTowards(transform.position, openPosition.position, Time.deltaTime * speed);
    }

    protected override void DeactivateAnimation()
    {
        throw new System.NotImplementedException();
    }

}

using UnityEngine;

public class Door : ActivableBase
{
    [Header("Position Settings")]
    [SerializeField]
    [Tooltip("Position of the door/bridge when it opens")]
    protected Transform openPosition;

    [Header("Movement Settings")]
    [SerializeField]
    [Tooltip("Speed to open the door/bridge")]
    private float speed = 2;

    protected override bool StopCondition()
    {
        // Define a small margin of error (epsilon)
        const float positionTolerance = 0.01f;
        bool isAtOpenPosition = Vector3.Distance(transform.position, openPosition.position) < positionTolerance;

        if (isAtOpenPosition)
        {
            Debug.Log("Door " + gameObject.name + " is at open position.");
        }

        return isAtOpenPosition;
    }

    protected override void ActivatedAction()
    {
        Debug.Log("Moving door " + gameObject.name + " towards open position...");
        transform.position = Vector3.MoveTowards(transform.position, openPosition.position, Time.deltaTime * speed);
    }

}

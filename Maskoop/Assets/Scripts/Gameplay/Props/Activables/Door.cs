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
        return transform.position.Equals(openPosition.position);
    }

    protected override void ActivatedAction()
    {
        transform.position = Vector3.MoveTowards(transform.position, openPosition.position, Time.deltaTime * speed);
    }

}

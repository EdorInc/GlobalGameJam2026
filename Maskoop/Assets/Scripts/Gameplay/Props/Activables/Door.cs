using UnityEngine;

public class Door : ActivableBase
{
    [Header("Position Settings")]
    [SerializeField]
    [Tooltip("Position of the door/bridge when it opens")]
    private Transform openPosition;

    [Header("Movement Settings")]
    [SerializeField]
    [Tooltip("Speed to open the door/bridge")]
    private float speed = 2;

    private bool shouldOpen = false;

    // Update is called once per frame
    void Update()
    {
        if (shouldOpen)
        {
            transform.position = Vector3.MoveTowards(transform.position, openPosition.position, Time.deltaTime * speed);
        }
        if (transform.position.Equals(openPosition.position))
        {
            shouldOpen = false;
        }
    }

    public override void OnActivatorRecived(int channel)
    {
        if (this.channel == channel)
        {
            activatorsNeeded--;
            if (activatorsNeeded == 0)
            {
                shouldOpen = true;
            }
        }
    }

}

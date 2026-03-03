using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Moving Settings")]
    [SerializeField] private float movingSpeed = 5;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float waitAtPointTime = 1;

    private Transform movingTowards;
    private bool waiting = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        movingTowards = pointA;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, movingTowards.position) < 0.01f && !waiting)
        {
            Invoke(nameof(DelayRestart), waitAtPointTime);
            waiting = true;
        }
        else if(!waiting)
        {
            transform.position = Vector3.MoveTowards(transform.position, movingTowards.position, movingSpeed * Time.fixedDeltaTime);
        }
    }

    void DelayRestart()
    {
        movingTowards = movingTowards == pointA ? pointB : pointA;
        waiting = false;
    }

}

using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Moving Settings")]
    [SerializeField] private float movingSpeed = 5;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float waitAtPointTime = 1;

    private Vector3 lastPosition;
    private Transform movingTowards;
    private bool waiting = false;
    private Rigidbody rb;

    public Vector3 Velociy { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        movingTowards = pointA;
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 distance = movingTowards.position - transform.position;
        Vector3 direction = distance.normalized;
        Debug.Log("Distancia:" + distance);
        if (distance.magnitude < 0.1f && !waiting)
        {
            Invoke(nameof(DelayRestart), waitAtPointTime);
            waiting = true;
        }
        else if(!waiting)
        {
            rb.linearVelocity = direction * movingSpeed;
        }
        Velociy = rb.linearVelocity;
    }

    void DelayRestart()
    {
        movingTowards = movingTowards == pointA ? pointB : pointA;
        waiting = false;
    }

}

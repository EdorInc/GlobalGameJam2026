using UnityEngine;

public class MovingObject : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private Transform pointA;
    [SerializeField]
    private Transform pointB;
    [SerializeField]
    private float stopTime;

    private Rigidbody rb;
    private Vector3 target;
    private Vector3 platformVelocity;

    public static bool canMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // platform moves manually
        target = pointB.position;
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        // Move the platform
        if (canMove)
        {
            Vector3 nextPos = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.fixedDeltaTime);
            platformVelocity = (nextPos - transform.position) / Time.fixedDeltaTime;
            rb.MovePosition(nextPos);

            if (Vector3.Distance(nextPos, target) < 0.01f)
            {
                target = target == pointA.position ? pointB.position : pointA.position;
                canMove = false;
                Invoke(nameof(CanMove), stopTime); 
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Pito");
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                collision.transform.SetParent(transform);
                break;
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        // Detach object when it leaves the platform
        if (collision.transform.parent == transform)
        {
            collision.transform.SetParent(null);
        }
    }
    public Vector3 GetVelocity()
    {
        return platformVelocity;
    }

    private void CanMove()
    {
        canMove = true;
    }
}

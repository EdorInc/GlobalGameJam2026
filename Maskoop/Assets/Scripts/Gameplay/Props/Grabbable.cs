using UnityEngine;
using UnityEngine.Rendering;

public class Grabbable : MonoBehaviour
{
    [Header("Optional Settings")]
    public Vector3 holdOffset = Vector3.zero;   // local position when held
    public Quaternion holdRotation = Quaternion.identity; // local rotation when held
    [Header("Throw force Settings")]
    public Vector2 maxThrowForce = new Vector2(8,8);
    public Vector2 minThrowForce = new Vector2(0, 0);
    public float forceGrowRate = 2;

    [Header("Ground Detection Settings")]
    public LayerMask groundLayer;
    public float rayDistance;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CheckGround();
    }

    private void CheckGround()
    {

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayDistance, groundLayer))
        {
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;
        }
    }
}
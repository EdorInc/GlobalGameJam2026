using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GroundDetector : MonoBehaviour
{
    private Collider col;

    [Header("Ground Detection Settings")]
    [SerializeField]
    [Tooltip("Layer(s) considered as ground for detection. If set to Nothing, ground detection will never activate.")]
    private LayerMask groundLayerMask;
    [SerializeField]
    [Tooltip("Distance below the collider to check for ground contact.")]
    private float groundCheckDistance = 0.1f;

    public bool IsGrounded { get; private set; }
    public GameObject MovingPlatform { get; private set; }

    protected virtual void Start()
    {
        col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("GroundDetector requires a Collider component.");
            throw new System.Exception("GroundDetector requires a Collider component.");
        }
    }

    private void FixedUpdate()
    {
        IsGrounded = false;
        MovingPlatform = null;

        if (col == null) return;

        // Small skin offset to avoid self-intersection.
        const float kSkin = 0.01f;

        // We'll use a spherecast whose radius matches the collider's foot radius.
        Vector3 rayStart;
        float castRadius;

        if (col is CapsuleCollider cap)
        {
            // Transform capsule parameters into world space
            Transform t = cap.transform;
            
            // Radius is affected by X/Z scale, height by Y scale.
            float radiusWorld = cap.radius * Mathf.Max(Mathf.Abs(t.lossyScale.x), Mathf.Abs(t.lossyScale.z));
            float heightWorld = cap.height * Mathf.Abs(t.lossyScale.y);
            
            // Center in world space
            Vector3 centerWorld = t.TransformPoint(cap.center);

            // Bottom center of the capsule in world space
            float halfHeight = Mathf.Max(heightWorld * 0.5f, radiusWorld);
            Vector3 bottom = centerWorld - t.up * (halfHeight - radiusWorld);

            rayStart = bottom + t.up * kSkin;
            castRadius = Mathf.Max(0.01f, radiusWorld * 0.9f);
        }
        else
        {
            // Fallback for other collider types, use bounds to place the start and a small radius.
            rayStart = col.bounds.center + Vector3.down * (col.bounds.extents.y - kSkin);
            castRadius = Mathf.Max(0.01f, Mathf.Min(col.bounds.extents.x, col.bounds.extents.z) * 0.5f);
        }

        // Debug visualization
        Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance, Color.green);

        if (Physics.SphereCast(rayStart, castRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayerMask))
        {
            IsGrounded = true;
            MovingPlatform = hit.collider.CompareTag("MovingPlatform") ? hit.collider.gameObject : null;
        }
    }
}
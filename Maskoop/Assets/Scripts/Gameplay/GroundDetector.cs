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

    [Header("Shadow Projection")]
    [SerializeField]
    [Tooltip("Prefab used as a projected shadow when the character is airborne. Can be null to disable.")]
    private GameObject shadowPrefab;
    [SerializeField]
    [Tooltip("Maximum distance below the character to search for ground to place the shadow.")]
    private float shadowMaxDistance = 20f;
    [SerializeField]
    [Tooltip("Small offset above the ground to avoid Z-fighting.")]
    private float shadowGroundOffset = 0.02f;

    private GameObject shadowInstance;

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

        UpdateShadowProjection();
    }

    private void UpdateShadowProjection()
    {
        // If no shadow prefab configured, ensure any existing instance is disabled and return.
        if (shadowPrefab == null)
        {
            if (shadowInstance != null)
            {
                shadowInstance.SetActive(false);
            }
            return;
        }

        // Only project a shadow while airborne
        if (IsGrounded)
        {
            if (shadowInstance != null)
            {
                shadowInstance.SetActive(false);
            }
            return;
        }

        // Cast downward from the character's position to find ground further below
        Vector3 rayOrigin = transform.position;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit shadowHit, shadowMaxDistance, groundLayerMask))
        {
            if (shadowInstance == null)
            {
                shadowInstance = Instantiate(shadowPrefab);
                shadowInstance.transform.SetParent(null);
            }

            shadowInstance.transform.position = shadowHit.point + shadowHit.normal * shadowGroundOffset;

            // Orient the shadow to be flush with the ground normal.
            Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, shadowHit.normal);
            if (projectedForward.sqrMagnitude <= 0.001f)
            {
                // Fallback forward if projection is degenerate
                projectedForward = Vector3.ProjectOnPlane(transform.up, shadowHit.normal);
            }
            shadowInstance.transform.rotation = Quaternion.LookRotation(projectedForward.normalized, shadowHit.normal);

            // Make sure the instance is active
            if (!shadowInstance.activeSelf)
            {
                shadowInstance.SetActive(true);
            }
        }
        else
        {
            // Nothing hit in range -> hide shadow
            if (shadowInstance != null && shadowInstance.activeSelf)
            {
                shadowInstance.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        if (shadowInstance != null)
        {
            shadowInstance.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (shadowInstance != null)
        {
            Destroy(shadowInstance);
        }
    }
}
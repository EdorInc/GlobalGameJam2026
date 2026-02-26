using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GroundDetector : MonoBehaviour
{
    private Collider col;

    [Header("Ground Detection Settings")]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundCheckDistance = 0.1f;

    public bool IsGrounded { get; private set; }

    protected virtual void Start()
    {
        col = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        IsGrounded = false;

        // Calculate bottom of collider in world space
        Vector3 colliderBottom = col.bounds.min;

        // Small offset to prevent self-intersection
        Vector3 rayStart = colliderBottom + Vector3.up * 0.01f;
        Vector3 rayDirection = Vector3.down;

        // Draw debug ray
        Debug.DrawRay(rayStart, rayDirection * groundCheckDistance, Color.green);

        if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, groundCheckDistance, groundLayerMask))
        {
            IsGrounded = true;
        }
    }
}
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Distance to check for ground when determining if the character is grounded.")]
    public float groundCheckDistance = 0.05f;
    [Tooltip("Layer mask to use when checking for ground.")]
    public LayerMask groundLayerMask = ~0;

    private CapsuleCollider capsuleCollider;

    public bool IsGrounded { get; private set; }

    private void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void FixedUpdate()
    {
        IsGrounded = false;

        float capsuleHeight = Mathf.Max(capsuleCollider.radius * 2f, capsuleCollider.height);
        Vector3 capsuleBottom = transform.TransformPoint(capsuleCollider.center - Vector3.up * capsuleHeight / 2f);
        float radius = transform.TransformVector(capsuleCollider.radius, 0f, 0f).magnitude;

        Vector3 rayStart = capsuleBottom + Vector3.up * .01f;
        Vector3 rayDirection = Vector3.down * groundCheckDistance;

        Ray ray = new Ray(rayStart, rayDirection);
        Debug.DrawRay(rayStart, rayDirection, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, groundCheckDistance, groundLayerMask))
        {
            IsGrounded = true;
        }
    }
}

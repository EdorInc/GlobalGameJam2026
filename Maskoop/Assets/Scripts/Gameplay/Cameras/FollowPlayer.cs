using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    private Transform otherTarget;

    [Header("Settings")]
    public float smoothTime = 0.1f;
    public float zoomOutOffset = 3f;
    public float zoomOutSpeed = 2f;

    [HideInInspector]
    public bool isMerged = false;

    private Vector3 originalPositionOffset;
    private Vector3 currentPositionOffset;
    private Vector3 velocity;

    void Start()
    {
        // Capture initial offsets
        originalPositionOffset = transform.position - target.position;
        currentPositionOffset = originalPositionOffset;
    }

    void LateUpdate()
    {
        Vector3 targetPos = target.position;

        if (isMerged && otherTarget != null)
        {
            // Calculate midpoint between the two targets
            targetPos = (target.position + otherTarget.position) / 2f;

            // Adjust offset and zoom out
            Vector3 zoomOffset = new Vector3(0, zoomOutOffset, 0);
            currentPositionOffset = Vector3.Lerp(currentPositionOffset, originalPositionOffset + zoomOffset, Time.deltaTime * zoomOutSpeed);
        }
        else
        {
            // Reset to original offset when not merged
            currentPositionOffset = Vector3.Lerp(currentPositionOffset, originalPositionOffset, Time.deltaTime * zoomOutSpeed);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos + currentPositionOffset,
            ref velocity,
            smoothTime
        );
    }

    public void SetOtherTarget(Transform other)
    {
        otherTarget = other;
    }
}
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    private Transform otherTarget;

    [Header("Settings")]
    public float smoothTime = 0.1f;
    private float zoomOutOffset = 0f;
    private float zoomSpeed = 0f;

    private Vector3 originalPositionOffset;
    private Vector3 currentPositionOffset;
    private Vector3 velocity;
    private bool offsetsInitialized;

    void LateUpdate()
    {
        if (target  == null) return;
        EnsureOffsetsInitialized();

        Vector3 targetPos = target.position;

        if (DynamicSplitManager.isMerged && otherTarget != null)
        {
            // Calculate midpoint between the two targets
            targetPos = (target.position + otherTarget.position) / 2f;

            // Adjust offset and zoom out
            Vector3 zoomOffset = new Vector3(0, zoomOutOffset, 0);
            currentPositionOffset = Vector3.Lerp(
                currentPositionOffset, 
                originalPositionOffset + zoomOffset, 
                Time.deltaTime * zoomSpeed
            );
        }
        else
        {
            // Reset to original offset when not merged
            currentPositionOffset = Vector3.Lerp(
                currentPositionOffset, 
                originalPositionOffset, 
                Time.deltaTime * zoomSpeed
            );
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

    public void SetZoomSettings(float zoomOutOffset, float zoomSpeed)
    {
        this.zoomOutOffset = zoomOutOffset;
        this.zoomSpeed = zoomSpeed;
    }

    private void EnsureOffsetsInitialized()
    {
        if (offsetsInitialized) return;

        originalPositionOffset = transform.position - target.position;
        currentPositionOffset = originalPositionOffset;
        offsetsInitialized = true;
    }
}
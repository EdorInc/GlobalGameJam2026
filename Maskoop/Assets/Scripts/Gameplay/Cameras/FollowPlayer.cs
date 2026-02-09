using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform target;

    [Header("Settings")]
    public float smoothTime = 0.1f;

    private Vector3 positionOffset;
    private Vector3 velocity;

    void Start()
    {
        // Capture initial offsets
        positionOffset = transform.position - target.position;
    }

    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            target.position + positionOffset,
            ref velocity,
            smoothTime
        );
    }
}
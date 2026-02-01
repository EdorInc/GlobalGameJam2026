using UnityEngine;

public class PlayerCameraInterface : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 10f;

    private CullingMaskController cameraCullingMaskController;
    private MaskManager maskManager;

    private Mask lastEquipedMask;

    private Vector3 initialOffset;
    private Quaternion initialRotation;

    // Vars for temp focus
    private Transform temporaryTarget;
    private float temporaryFocusEndTime;

    void Start()
    {
        if (!target)
        {
            Debug.LogWarning("No target assigned!");
            return;
        }

        // Save the rotation the camera has in the scene
        initialRotation = transform.rotation;
        initialOffset = transform.position - target.transform.position;

        cameraCullingMaskController = GetComponent<CullingMaskController>();
        maskManager = target.GetComponent<MaskManager>();

        lastEquipedMask = maskManager.GetCurrentMask();
    }

    void LateUpdate()
    {
        if (!target) return;

        // Lock rotation to the initial one
        transform.rotation = initialRotation;

        Transform currentTarget = GetCurrentTarget();

        // Smooth position follow
        transform.position = Vector3.Lerp(
            transform.position,
            currentTarget.position + initialOffset,
            followSpeed * Time.deltaTime
        );

        Mask currentMask = maskManager.GetCurrentMask();

        if(currentMask != lastEquipedMask)
        {
            lastEquipedMask = currentMask;
            cameraCullingMaskController.ApplyView(currentMask);
        }
    }

    private Transform GetCurrentTarget()
    {
        if (temporaryTarget != null && Time.time < temporaryFocusEndTime)
        {
            return temporaryTarget;
        }
        temporaryTarget = null;
        return target;
    }

    public void FocusOnTarget(Transform transform, float duration)
    {
        temporaryTarget = transform;
        temporaryFocusEndTime = Time.time + duration;
    }

    public void AssignTarget(Transform transform)
    {
        target = transform;
    }
}
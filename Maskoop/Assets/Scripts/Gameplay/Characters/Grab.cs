using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class Grab : MonoBehaviour
{
    public enum GrabRaycastMode
    {
        /// <summary>
        /// Casts rays in a cone pattern (fan out from origin).
        /// </summary>
        Cone,
        /// <summary>
        /// Casts rays in a cross pattern (center and four offsets).
        /// </summary>
        Cross
    }

    [HideInInspector]
    public GameObject grabbedObject = null;

    [HideInInspector]
    public GameObject highlightedObject = null;

    [Header("Grab Settings")]
    [SerializeField]
    [Tooltip("Forward offset from the player position to start the grab raycast. Adjust to control how far in front of the player the grab begins.")]
    private float grabForwardOffset = 0.1f;
    [SerializeField]
    [Tooltip("Upward offset from the player position to start the grab raycast. Adjust to control the vertical position of the grab origin.")]
    private float grabUpOffset = 0.3f;
    [SerializeField]
    [Tooltip("Selects the raycast pattern used to detect grabbable objects.")]
    private GrabRaycastMode grabRaycastMode = GrabRaycastMode.Cone;
    [SerializeField]
    [Tooltip("Maximum distance from the player at which objects can be grabbed.")]
    private float grabRange = 3f;
    [SerializeField]
    [Tooltip("Radius used for the cone or cross raycast pattern. Controls the spread of the grab detection.")]
    private float grabRadius = 0.5f;
    [SerializeField]
    [Tooltip("Layer(s) that can be grabbed. Only objects on these layers will be detected by the grab raycast.")]
    private LayerMask grabMask;

    [Header("Hold Settings")]
    [SerializeField]
    [Tooltip("Transform representing the position and rotation where the grabbed object will be held.")]
    private Transform grabbedPosition;

    [Header("Debug Settings")]
    [SerializeField]
    [Tooltip("Draw grab ray gizmos in the Scene view for debugging.")]
    private bool showGrabGizmos = true;

    private Equip equipComponent;
    private CharacterStateController characterState;

    private void Start()
    {
        equipComponent = GetComponent<Equip>();
        characterState = GetComponent<CharacterStateController>();
    }

    private void OnEnable()
    {
        EventManager.OnTryingToBeFree += DropPlayer;
    }

    private void OnDisable()
    {
        EventManager.OnTryingToBeFree -= DropPlayer;
    }

    void Update()
    {
        HighlightObject();
    }

    void LateUpdate()
    {
        if (grabbedObject != null)
        {
            Grabbable grabbable = grabbedObject.GetComponent<Grabbable>();

            // Move smoothly to the hold position
            Vector3 targetPos = grabbedPosition.position + grabbable.holdOffset;
            Quaternion targetRot = grabbedPosition.rotation * grabbable.holdRotation;

            grabbedObject.transform.position = targetPos;
            grabbedObject.transform.rotation = targetRot;
        }
    }

    public GameObject FindObject()
    {
        // Raycast a sphere to find nearby objects to grab
        Vector3 origin = transform.position + transform.forward.normalized * grabForwardOffset + Vector3.up * grabUpOffset;
        Vector3 direction = transform.forward.normalized;

        RaycastHit hit;
        bool hitFound;

        switch (grabRaycastMode)
        {
            case GrabRaycastMode.Cone:
                hitFound = RaycastCone(origin, direction, grabRange, grabRadius, out hit);
                break;
            case GrabRaycastMode.Cross:
                hitFound = RaycastCross(origin, direction, grabRange, grabRadius, out hit);
                break;
            default:
                Debug.LogWarning("Using default raycast mode due to unrecognized setting.");
                hitFound = RaycastCone(origin, direction, grabRange, grabRadius, out hit);
                break;
        }

        return hitFound ? hit.collider.gameObject : null;
    }

    public void HighlightObject()
    {
        if (grabbedObject != null)
        {
            return;
        }

        GameObject grabbableObject = FindObject();

        if (grabbableObject != highlightedObject)
        {
            if (grabbableObject)
            {
                grabbableObject.GetComponent<Grabbable>().Highlight(true);
            }

            if (highlightedObject)
            {
                highlightedObject.GetComponent<Grabbable>().Highlight(false);
            }

            highlightedObject = grabbableObject;

            if(highlightedObject != null)
            {
                Debug.Log("New grabbable object in range: " + (highlightedObject != null ? highlightedObject.name : "None"));
            }
        }
    }

    public void GrabObject()
    {
        if (grabbedObject != null)
        {
            Debug.Log("Already grabbing an object.");

            DropObject();

            return;
        }

        // GameObject grabbableObject = FindObject();

        // Custom cast to detect objects in front
        if (highlightedObject != null)
        {
            highlightedObject.GetComponent<Grabbable>().Highlight(false);
            grabbedObject = highlightedObject;

            Grabbable grabbable = grabbedObject.GetComponent<Grabbable>();

            if (grabbable == null)
            {
                Debug.Log("Hit object " + grabbedObject.name + " is not grabbable.");
                return;
            }

            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();

            if (rb == null)
            {
                Debug.Log("Hit object " + grabbedObject.name + " has not Rigidbody.");
                return;
            }

            Collider collider = grabbedObject.GetComponent<Collider>();

            if (collider == null)
            {
                Debug.Log("Hit object " + grabbedObject.name + " has not Collider.");
                return;
            }

            rb.isKinematic = true;
            rb.useGravity = false;

            collider.enabled = false;

            grabbable.IsGrabbed = true;

            if (grabbable.gameObject.CompareTag("Player"))
            {
                grabbable.gameObject.GetComponent<CharacterStateController>().SetBeingGrabbed(true);
            }

            if (grabbable.gameObject.CompareTag("Mask"))
            {
                equipComponent.ChangeEquipState();
            }
            else
            {
                characterState.SetHeldObject(grabbable);
                Debug.Log("Grabbed " + grabbedObject.name);
                AudioSystem.PlaySFX(AudioSystem.SoundLibrary?.grab, transform.position);
            }
        }
        else
        {
            Debug.Log("No grabbable object in range.");
            EventManager.OnCantPerforAction?.Invoke(gameObject);

        }

    }

    /// <summary>
    /// Attempts a raycast from the specified origin in the given direction to detect a Grabbable object.
    /// Used internally by the raycast methods to launch individual rays and draw debug lines.
    /// Draws a debug line for visualization. Only returns true if the hit collider has a Grabbable component.
    /// If the hit object is not grabbable, the hit is ignored and the method returns false.
    /// </summary>
    bool TryRaycast(Vector3 rayOrigin, Vector3 rayDirection, float range, Color debugColor, out RaycastHit hit)
    {
        hit = default;

#if UNITY_EDITOR
        if (showGrabGizmos)
        {
            Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * range, debugColor, 0f, false);
        }
#endif

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, range, grabMask))
        {
            Grabbable grabbable = hit.collider.GetComponent<Grabbable>();
            if (grabbable != null)
            {
                return true;
            } else { 
                hit = default;
            }
            
        }

        return false;
    }

    /// <summary>
    /// Performs a cross-pattern raycast to detect grabbable objects.
    /// Casts a central ray and four additional rays offset above, below, left, and right from the origin.
    /// Returns true if any ray hits a valid Grabbable object within the specified range and radius.
    /// </summary>
    /// <param name="origin">The starting point of the rays.</param>
    /// <param name="direction">The forward direction for the rays.</param>
    /// <param name="range">The maximum distance to check for collisions.</param>
    /// <param name="radius">The offset distance for the cross pattern.</param>
    /// <param name="hit">The RaycastHit information for the first valid hit.</param>
    /// <returns>True if a grabbable object is detected; otherwise, false.</returns>
    bool RaycastCross(Vector3 origin, Vector3 direction, float range, float radius, out RaycastHit hit)
    {
        hit = default;
        direction = direction.normalized;

        // Center ray
        if (TryRaycast(origin, direction, range, Color.red, out hit))
            return true;

        // Cross pattern offsets
        Vector3[] offsets = GetCrossOffsets(origin, transform, radius);

        Color debugColor = Color.yellow;

        foreach (var offset in offsets)
        {
            if (TryRaycast(offset, direction, range, Color.yellow, out hit))
                return true;
        }

        // No hit found
        return false;
    }
    
    /// <summary>
    /// Performs a cone-pattern raycast to detect grabbable objects.
    /// Casts a central ray and four additional rays from the origin towards points offset around the end of the cone.
    /// Returns true if any ray hits a valid Grabbable object within the specified range and radius.
    /// </summary>
    /// <param name="origin">The starting point of the rays.</param>
    /// <param name="direction">The forward direction for the cone.</param>
    /// <param name="range">The maximum distance to check for collisions.</param>
    /// <param name="radius">The offset distance for the cone pattern at the end point.</param>
    /// <param name="hit">The RaycastHit information for the first valid hit.</param>
    /// <returns>True if a grabbable object is detected; otherwise, false.</returns>
    bool RaycastCone(Vector3 origin, Vector3 direction, float range, float radius, out RaycastHit hit)
    {
        hit = default;
        direction = direction.normalized;

        Vector3 end = origin + direction * range;

        // Center ray
        if (TryRaycast(origin, direction, range, Color.red, out hit))
            return true;

        // Circular pattern offsets
        Vector3[] endOffsets = GetConeOffsets(end, transform, radius);

        Color debugColor = Color.yellow;

        foreach (var endOffset in endOffsets)
        {
            Vector3 rayDirection = (endOffset - origin).normalized;

            if (TryRaycast(origin, rayDirection, range, Color.yellow, out hit))
                return true;
        }

        // No hit found
        return false;
    }

    public void DropPlayer(GameObject player)
    {
        if (player == grabbedObject && !characterState.IsChargingThrow)
        {
            DropObject();
        }
    }
    
    public void DropObject()
    {
        if (grabbedObject == null)
        {
            if (equipComponent.IsMaskEquiped())
            {
                equipComponent.ChangeEquipState();
            }
            else
            {
                return;
            }
        }

        if (characterState.IsChargingThrow)
        {
            return;
        }

        Collider collider = grabbedObject.GetComponent<Collider>();

        if (collider != null)
        {
            collider.enabled = true;
        }

        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();

        characterState.SetHeldObject(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        grabbedObject.GetComponent<Grabbable>().IsGrabbed = false;
        if (grabbedObject.CompareTag("Player"))
        {
            grabbedObject.GetComponent<CharacterStateController>().SetBeingGrabbed(false);
        }
        grabbedObject = null;
    }

    /// <summary>
    /// Returns offsets for the cross raycast pattern.
    /// </summary>
    private static Vector3[] GetCrossOffsets(Vector3 origin, Transform t, float radius)
    {
        return new Vector3[]
        {
            origin + t.up * radius,
            origin - t.up * radius,
            origin + t.right * radius,
            origin - t.right * radius,
            origin + t.up * (radius * 0.5f),
            origin - t.up * (radius * 0.5f),
            origin + t.right * (radius * 0.5f),
            origin - t.right * (radius * 0.5f)
        };
    }

    /// <summary>
    /// Returns offsets for the cone raycast pattern.
    /// </summary>
    private static Vector3[] GetConeOffsets(Vector3 end, Transform t, float radius)
    {
        return new Vector3[]
        {
            end + t.up * radius,
            end - t.up * radius,
            end + t.right * radius,
            end - t.right * radius,
            end + t.up * (radius * 0.5f),
            end - t.up * (radius * 0.5f),
            end + t.right * (radius * 0.5f),
            end - t.right * (radius * 0.5f),
            end + (t.up + t.right).normalized * radius,
            end + (t.up - t.right).normalized * radius,
            end + (-t.up + t.right).normalized * radius,
            end + (-t.up - t.right).normalized * radius
        };
    }

}

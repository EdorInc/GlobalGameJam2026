using UnityEngine;

public class Grab : MonoBehaviour
{
    [HideInInspector]
    public GameObject grabbedObject = null;

    [Header("Grab Settings")]
    [SerializeField] private float grabOffset = 0.1f; // Distance in front of the player to check for grabbable objects
    [SerializeField] private float grabRange = 3f;
    [SerializeField] private float grabRadius = 0.5f;
    [SerializeField] private LayerMask grabMask;

    [Header("Hold Settings")]
    [SerializeField] private Transform grabbedPosition;
    [SerializeField] private Vector3 dropOffsetPosition = new Vector3(0, 0.5f, 0); 

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

    public void GrabObject()
    {
        if (grabbedObject != null)
        {
            Debug.Log("Already grabbing an object.");

            DropObject();

            return;
        }

        // Raycast a sphere to find nearby objects to grab
        Vector3 origin = transform.position + transform.forward.normalized * grabOffset;
        Vector3 direction = transform.forward.normalized;

        RaycastHit hit;

        // SphereCast to detect objects in front
        if (RaycastCross(origin, direction, grabRange, grabRadius, out hit))
        {
            // Check if the object has a Grabbable component
            Grabbable grabbable = hit.collider.GetComponent<Grabbable>();
            if (grabbable == null)
            {
                Debug.Log("Object is not grabbable: " + hit.collider.name);
                return;
            }

            grabbedObject = grabbable.gameObject;
            

            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            else
            {
                grabbedObject = null;
                return;
            }

            Collider collider = grabbedObject.GetComponent<Collider>();

            if (collider != null)
            {
                collider.enabled = false;
                return;
            }

            Debug.Log("Grabbed object " + grabbedObject.name);
        }
        else
        {
            Debug.Log("No grabbable object in range.");
        }

    }

    bool RaycastCross(Vector3 origin, Vector3 direction, float range, float radius, out RaycastHit hit)
    {
        hit = default;

        direction = direction.normalized; // Make sure direction is normalized

        // Center ray
        Debug.DrawLine(origin, origin + direction * range, Color.red, 0.1f);
        if (Physics.Raycast(origin, direction, out hit, range, grabMask))
            return true;

        // Cross pattern offsets
        Vector3[] offsets = new Vector3[]
        {
        origin + transform.up * radius,       // above
        origin - transform.up * radius,       // below
        origin + transform.right * radius,    // right
        origin - transform.right * radius     // left
        };

        Color debugColor = Color.yellow;

        foreach (var offset in offsets)
        {
            // Draw debug line for each offset
            Debug.DrawLine(offset, offset + direction * range, debugColor, 0.1f);

            if (Physics.Raycast(offset, direction, out hit, range, grabMask))
                return true;
        }

        // No hit found
        return false;
    }

    

    public void DropObject()
    {
        if (grabbedObject == null) return;

        Collider collider = grabbedObject.GetComponent<Collider>();

        if (collider != null)
        {
            collider.enabled = true;
        }

        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        grabbedObject = null;
    }
}

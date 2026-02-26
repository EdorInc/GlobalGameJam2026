using UnityEngine;

public class Grab : MonoBehaviour
{
    [HideInInspector]
    public GameObject grabbedObject = null;

    [Header("Grab Settings")]
    public float grabRange = 3f;
    public float grabRadius = 0.5f;
    public LayerMask grabMask;

    [Header("Hold Settings")]
    [SerializeField]
    private Transform grabbedPosition;

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
        Vector3 origin = grabbedPosition.position;
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
                Debug.LogError("Grabbed object has no Rigidbody!");
                grabbedObject = null;
                return;
            }

            //BoxCollider boxCollider = grabbedObject.GetComponent<BoxCollider>();
            Collider collider = grabbedObject.GetComponent<Collider>();

            if (collider != null)
            {
                collider.enabled = false;
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
        origin + Vector3.up * radius,
        origin + Vector3.down * radius,
        origin + Vector3.right * radius,
        origin + Vector3.left * radius
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

        //BoxCollider boxCollider = grabbedObject.GetComponent<BoxCollider>();
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

using UnityEngine;

public class Grab : MonoBehaviour
{   
    [Header("Grab Settings")]
    public float grabRange = 3f;
    public float grabRadius = 0.5f;
    public LayerMask grabMask;

    [Header("Hold Settings")]
    [SerializeField]
    private Transform grabbedPosition;

    private GameObject grabbedObject = null;

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
            return;
        }

        // Raycast a sphere to find nearby objects to grab
        Vector3 origin = grabbedPosition.position;
        Vector3 direction = transform.forward.normalized;

        RaycastHit hit;

        DebugDrawSphereCast(origin, grabRadius, direction, grabRange);

        // SphereCast to detect objects in front
        if (Physics.SphereCast(origin, grabRadius, direction, out hit, grabRange))
        {

            Debug.DrawLine(origin, hit.point, Color.green);

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

            BoxCollider boxCollider = grabbedObject.GetComponent<BoxCollider>();

            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }

            Debug.Log("Grabbed object " + grabbedObject.name);
        }
        else
        {
            Debug.Log("No grabbable object in range.");
        }

    }

    void DebugDrawSphereCast(Vector3 origin, float radius, Vector3 direction, float distance, int steps = 10)
    {
        // Draw a line along the cast
        Debug.DrawLine(origin, origin + direction * distance, Color.red);

        // Reduce radius for better visualization
        radius = radius / 2;

        // Draw spheres along the path to approximate the cast volume
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 point = origin + direction * t * distance;

            Vector3 position = point;
            Color color = Color.yellow;
            float duration = 0.1f;

            Debug.DrawLine(position + Vector3.up * radius, position - Vector3.up * radius, color, duration);
            Debug.DrawLine(position + Vector3.right * radius, position - Vector3.right * radius, color, duration);
            Debug.DrawLine(position + Vector3.forward * radius, position - Vector3.forward * radius, color, duration);
        }
    }

    public void DropObject()
    {
        if (grabbedObject == null) return;

        BoxCollider boxCollider = grabbedObject.GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            boxCollider.enabled = true;
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

using System.Net;
using UnityEngine;
using UnityEngine.UIElements;

public class Grab : MonoBehaviour
{
    [HideInInspector]
    public GameObject grabbedObject = null;

    [Header("Grab Settings")]
    [SerializeField] private float grabForwardOffset = 0.1f;
    [SerializeField] private float grabUpOffset = 0.3f;
    [SerializeField] private bool useGrabCross = false;
    [SerializeField] private float grabRange = 3f;
    [SerializeField] private float grabRadius = 0.5f;
    [SerializeField] private LayerMask grabMask;

    [Header("Hold Settings")]
    [SerializeField] private Transform grabbedPosition;


    private Equip equipComponent;
    private CharacterStateController characterState;

    private void Start()
    {
        equipComponent = GetComponent<Equip>();
        characterState = GetComponent<CharacterStateController>();
    }

    private void OnEnable()
    {
        EventManager.TryingToBeFree += DropPlayer;
    }

    private void OnDisable()
    {
        EventManager.TryingToBeFree -= DropPlayer;
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

    public void GrabObject()
    {
        if (grabbedObject != null)
        {
            Debug.Log("Already grabbing an object.");

            DropObject();

            return;
        }

        // Raycast a sphere to find nearby objects to grab
        Vector3 origin = transform.position + transform.forward.normalized * grabForwardOffset + Vector3.up * grabUpOffset;
        Vector3 direction = transform.forward.normalized;

        RaycastHit hit;
        bool hitFound;

        if (useGrabCross)
            hitFound = RaycastCross(origin, direction, grabRange, grabRadius, out hit);
        else
            hitFound = RaycastCone(origin, direction, grabRange, grabRadius, out hit);

        // Custom cast to detect objects in front
        if (hitFound)
        {
            Grabbable grabbable = hit.collider.GetComponent<Grabbable>();

            if (grabbable == null)
            {
                Debug.Log("Hit object " + hit.collider.name + " is not grabbable.");
                return;
            }

            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();

            if (rb == null)
            {
                Debug.Log("Hit object " + hit.collider.name + " has not Rigidbody.");
                return;
            }

            Collider collider = hit.collider.GetComponent<Collider>();

            if (collider == null)
            {
                Debug.Log("Hit object " + hit.collider.name + " has not Collider.");
                return;
            }

            rb.isKinematic = true;
            rb.useGravity = false;

            collider.enabled = false;

            grabbedObject = grabbable.gameObject;

            grabbable.IsGrabbed = true;

            characterState.SetHeldObject(grabbable);

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
                Debug.Log("Grabbed object " + grabbedObject.name);

            }
        }
        else
        {
            Debug.Log("No grabbable object in range.");
            EventManager.OnCantPerforAction?.Invoke(gameObject);

        }

    }
    bool TryRaycast(Vector3 rayOrigin, Vector3 rayDirection, float range, Color debugColor, out RaycastHit hit)
    {
        hit = default;

        Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * range, debugColor, 0.1f);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, range, grabMask))
        {
            Grabbable grabbable = hit.collider.GetComponent<Grabbable>();
            if (grabbable != null)
            {
                return true;
            } else { 
                // Debug.LogWarning("Skipped object " + hit.collider.name + ".");
                hit = default;
            }
            
        }

        return false;
    }

    bool RaycastCross(Vector3 origin, Vector3 direction, float range, float radius, out RaycastHit hit)
    {
        hit = default;
        direction = direction.normalized;

        // Center ray
        if (TryRaycast(origin, direction, range, Color.red, out hit))
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
            if (TryRaycast(offset, direction, range, Color.yellow, out hit))
                return true;
        }

        // No hit found
        return false;
    }

    bool RaycastCone(Vector3 origin, Vector3 direction, float range, float radius, out RaycastHit hit)
    {
        hit = default;
        direction = direction.normalized;

        Vector3 end = origin + direction * range;

        // Center ray
        if (TryRaycast(origin, direction, range, Color.red, out hit))
            return true;

        // Cross pattern offsets
        Vector3[] endOffsets = new Vector3[]
        {
        end + transform.up * radius,       // above
        end - transform.up * radius,       // below
        end + transform.right * radius,    // right
        end - transform.right * radius     // left
        };

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
        if (player == grabbedObject)
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


}

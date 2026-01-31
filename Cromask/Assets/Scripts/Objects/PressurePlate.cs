using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [Header("Plate Settings")]
    [SerializeField] private float pressedHeight = 0.1f;
    [SerializeField] private float pressSpeed = 5f;

    [Header("Cube Detection")]
    [SerializeField] private string cubeTag = "Cube";
    [SerializeField] private Vector3 cubeLocalPosition = new Vector3(0, 0.8f, 0); // Posición relativa al padre de la placa
    [SerializeField] private float repositionSpeed = 8f;

    [Header("Events")]
    [SerializeField] private UnityEvent onPlateActivated;
    [SerializeField] private UnityEvent onPlateDeactivated;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Vector3 pressedPosition;
    private Transform parent;
    private GameObject lockedCube = null;
    private bool isPlateActive = false;

    private void Start()
    {
        parent = transform.parent;
        originalPosition = parent.position;
        targetPosition = originalPosition;
        pressedPosition = originalPosition - Vector3.up * pressedHeight;
    }

    private void Update()
    {
        parent.position = Vector3.Lerp(parent.position, targetPosition, Time.deltaTime * pressSpeed);
    }

    private void FixedUpdate()
    {
        if (lockedCube != null)
        {
            Rigidbody rb = lockedCube.GetComponent<Rigidbody>();
            if (rb && rb.isKinematic)
            {
                Vector3 desiredWorldPosition = pressedPosition + cubeLocalPosition;
                lockedCube.transform.position = desiredWorldPosition;
                lockedCube.transform.rotation = Quaternion.identity;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPlateActive && other.gameObject.CompareTag(cubeTag))
        {
            GrabableObject grabable = other.gameObject.GetComponent<GrabableObject>();
            if (grabable && !grabable.IsGrabbed())
            {
                ActivatePlate(other.gameObject, grabable);
            }
        }
    }

    private void ActivatePlate(GameObject cube, GrabableObject grabable)
    { 
        isPlateActive = true;
        lockedCube = cube;

        targetPosition = pressedPosition;
        onPlateActivated?.Invoke();

        lockedCube.layer = LayerMask.NameToLayer("Ignore Raycast");
        grabable.enabled = false;

        Rigidbody rb = lockedCube.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
        }

        Collider cubeCollider = lockedCube.GetComponent<Collider>();
        if (cubeCollider != null)
        {
            cubeCollider.isTrigger = true;
        }

        // Posicionar inmediatamente
        Vector3 finalPosition = pressedPosition + cubeLocalPosition;
        lockedCube.transform.position = finalPosition;
        lockedCube.transform.rotation = Quaternion.identity;

        Debug.Log("Placa presionada - Cubo bloqueado");
    }
}

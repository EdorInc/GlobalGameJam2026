using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.InputSystem.InputAction;

public class GrabAction : MonoBehaviour
{
    [SerializeField]
    private LayerMask mask;
    [SerializeField]
    private LayerMask maskWithRed;
    [SerializeField] 
    bool useRedMask = false;
    [SerializeField]
    private float minForceFowards = 0;
    [SerializeField]
    private float minForceUp = 0;
    [SerializeField]
    private float maxForceFoward = 8;
    [SerializeField]
    private float maxForceUp = 8;
    [SerializeField]
    private float forceGrow = 2;
    [SerializeField]
    private float holdDistance = 3;
    [SerializeField]
    private Vector3 holdPosition = Vector3.forward;
    [SerializeField] 
    private float maxTime = 5.0f;
    [SerializeField]
    private float timeStep = 0.1f;
    [SerializeField]
    private float lineWidth = 0.06f;
    [SerializeField]
    private float grabHeight = 0.5f;

    private GameObject grabbedObject = null;
    private float currentForceFoward;
    private float currentForceUp;
    private bool charging = false;
    private LineRenderer line;

    private MaskManager maskManager;
    private Mask lastEquipedMask;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    void Start()
    {
        maskManager = GetComponent<MaskManager>();

        lastEquipedMask = maskManager.GetCurrentMask();
        Clear();
    }

    void Update()
    {
        if (charging)
        {
            Charge();
        }

        Mask currentMask = maskManager.GetCurrentMask();

        if (currentMask != lastEquipedMask)
        {
            lastEquipedMask = currentMask;
            useRedMask = currentMask == Mask.Red;
        }
    }

    public void GrabObject()
    {
        if (grabbedObject != null) return;

        RaycastHit hit;

        LayerMask maskToDetect = useRedMask ? maskWithRed : mask;

        Vector3 position = transform.position + Vector3.down * grabHeight;

        Debug.DrawLine(position, position + transform.forward * holdDistance,Color.red,5);

        if (!Physics.Raycast(position, transform.forward, out hit, holdDistance, maskToDetect))
        {
            Debug.Log("Nothing detected in front.");
            return;
        }

        Debug.Log("Grabbed: " + hit.collider.name);

        grabbedObject = hit.collider.gameObject;

        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Object has no Rigidbody");
            grabbedObject = null;
            return;
        }

        grabbedObject.transform.parent = transform;
        grabbedObject.transform.rotation = transform.rotation;
        grabbedObject.transform.localPosition = holdPosition;
        grabbedObject.GetComponent<BoxCollider>().enabled = false;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    public void StartCharge()
    {
        if (!charging && grabbedObject != null)
        {
            charging = true;
            currentForceUp = minForceUp;
            currentForceFoward = minForceFowards;
        }
    }

    private void Charge()
    {
        currentForceFoward += forceGrow * Time.deltaTime;
        currentForceUp += forceGrow * Time.deltaTime;
        currentForceFoward = Mathf.Min(currentForceFoward, maxForceFoward);
        currentForceUp = Mathf.Min(currentForceUp, maxForceUp);
        DrawTrajectory(currentForceFoward, currentForceUp);
    }


    public void ThrowObject()
    {
        grabbedObject.GetComponent<Rigidbody>().isKinematic = false;

        if (grabbedObject == null)
        {
            return;
        }
        Rigidbody rbCube = grabbedObject.GetComponent<Rigidbody>();
        if (rbCube != null) 
        {
            Debug.Log("Thrown: " + grabbedObject.name);

            charging = false;
            rbCube.useGravity = true;
            grabbedObject.transform.parent = grabbedObject.GetComponent<GrabableObject>().GetBaseParent();
            rbCube.AddForce(transform.forward * currentForceFoward + Vector3.up * currentForceUp, ForceMode.Impulse);
            grabbedObject.GetComponent<BoxCollider>().enabled = true;
            grabbedObject = null;

            Clear();
        }
    }

    public GameObject GetGrabbedObject()
    {
        GameObject grabableObject = grabbedObject;
        return grabableObject;
    }

    public void RemoveGrabbedObject()
    {
        grabbedObject = null;
    }

    public void ForceGrabObject(GameObject obj)
    {
        grabbedObject = obj;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        grabbedObject.GetComponent<BoxCollider>().enabled = false;

        obj.transform.SetParent(transform);
        obj.transform.localPosition = holdPosition;
        obj.transform.localRotation = Quaternion.identity;
    }


    public void DrawTrajectory(float impulseStrengthFoward, float impulseStrengthUp)
    {
        Vector3 initialPos = grabbedObject.transform.position;
        Vector3 initialVelocity = transform.forward * (impulseStrengthFoward / grabbedObject.GetComponent<Rigidbody>().mass) +
            Vector3.up * (impulseStrengthUp / grabbedObject.GetComponent<Rigidbody>().mass);

        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        Vector3 previousPoint = initialPos;
        int pointsDrawn = 0;
        bool hitGround = false;

        int maxSteps = Mathf.CeilToInt(maxTime / timeStep);

        line.positionCount = maxSteps;

        for (int i = 0; i < maxSteps; i++)
        {
            float t = i * timeStep;

            Vector3 currentPoint =
                initialPos +
                initialVelocity * t +
                0.5f * Physics.gravity * t * t;

            // Raycast between last point and this point
            Vector3 segment = currentPoint - previousPoint;
            float distance = segment.magnitude;

            if (Physics.Raycast(previousPoint, segment.normalized, out RaycastHit hit, distance))
            {
                // Stop at ground hit
                line.SetPosition(pointsDrawn, hit.point);
                pointsDrawn++;
                hitGround = true;
                break;
            }

            line.SetPosition(pointsDrawn, currentPoint);

            previousPoint = currentPoint;
            pointsDrawn++;
        }

        line.positionCount = pointsDrawn - 1;
    }
    public void Clear()
    {
        line.positionCount = 0;
    }

}

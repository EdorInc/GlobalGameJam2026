using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class GrabAction : MonoBehaviour
{
    [SerializeField]
    private LayerMask mask;
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
    private int resolution = 30;
    [SerializeField]
    private float timeStep = 0.1f;

    private GameObject grabbedObject = null;
    private float currentForceFoward;
    private float currentForceUp;
    private bool charging = false;
    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (charging)
        {
            Charge();
        }
    }

    public void GrabObject()
    {
        if (grabbedObject != null) return;

        RaycastHit hit;


        if (!Physics.Raycast(transform.position, transform.forward, out hit, holdDistance, mask))
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
        rb.useGravity = false;
    }

    public void StartCharge()
    {
        if (!charging)
        {
            charging = true;
            currentForceUp = minForceUp;
            currentForceFoward = minForceFowards;
        }
    }
    public void ThrowObject()
    {
        Rigidbody rbCube = grabbedObject.GetComponent<Rigidbody>();
        if (rbCube != null) 
        {
            Debug.Log("Thrown: " + grabbedObject.name);

            charging = false;
            rbCube.useGravity = true;
            grabbedObject.transform.parent = grabbedObject.GetComponent<GrabableObject>().GetBaseParent();
            rbCube.AddForce(transform.forward * currentForceFoward + Vector3.up * currentForceUp, ForceMode.Impulse);
            grabbedObject = null;
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

    public GameObject GetGrabbedObject()
    {
        return grabbedObject;
    }

    public void DrawTrajectory(float impulseStrengthFoward,float impulseStrengthUp)
    {
        Vector3 startPos = grabbedObject.transform.position;

        Vector3 initialVelocity = transform.forward * (impulseStrengthFoward / grabbedObject.GetComponent<Rigidbody>().mass) +
            Vector3.up * (impulseStrengthUp / grabbedObject.GetComponent<Rigidbody>().mass);

        line.positionCount = resolution;

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;

            Vector3 point =
                startPos +
                initialVelocity * t +
                0.5f * Physics.gravity * t * t;

            line.SetPosition(i, point);
        }
    }
    public void Clear()
    {
        line.positionCount = 0;
    }

}

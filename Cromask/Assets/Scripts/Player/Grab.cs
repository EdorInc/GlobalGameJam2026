using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class Grab : MonoBehaviour
{
    [SerializeField]
    private LayerMask mask;
    [SerializeField]
    private float minForce = 0;
    [SerializeField]
    private float maxForce = 100;
    [SerializeField]
    private float forceGrow = 100;
    [SerializeField]
    private float holdDistance = 3;
    [SerializeField]
    private Vector3 holdPosition = Vector3.forward;

    private GameObject grabbedObject = null;
    private float currentForce;
    private bool charging = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        
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

        Debug.DrawRay(transform.position, transform.forward * holdDistance, Color.red,5);

        if (!Physics.Raycast(transform.position, transform.forward, out hit, holdDistance, mask))
        {
            Debug.Log("Nothing hit");
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
            currentForce = minForce;
        }
    }
    public void ThrowObject()
    {
        Rigidbody rbCube = grabbedObject.GetComponent<Rigidbody>();
        Debug.Log("Lanzado a:" + currentForce);
        if (rbCube != null) 
        {
            charging = false;
            rbCube.useGravity = true;
            grabbedObject.transform.parent = grabbedObject.GetComponent<GrabableObject>().GetBaseParent();
            rbCube.AddForce(transform.forward * currentForce + Vector3.up * currentForce,ForceMode.Impulse);
            grabbedObject = null;
        }
    }

    private void Charge()
    {
        Debug.Log(currentForce);
        currentForce += forceGrow * Time.deltaTime;
        if(currentForce > maxForce)
        {
            currentForce = maxForce;
        }
    }

}

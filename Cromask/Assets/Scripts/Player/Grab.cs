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

    private GameObject grabbedObject;
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
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.position + holdDistance * transform.forward, out hit, mask))
        {
            Debug.DrawLine(transform.position, transform.position + holdDistance * transform.forward);
            Debug.Log("Hola");
            grabbedObject = hit.transform.gameObject;
            grabbedObject.transform.parent = transform;
            Rigidbody rbCube = grabbedObject.GetComponent<Rigidbody>();
            rbCube.useGravity = false;

        }
    }

    public void StartCharge()
    {
        charging = true;
        currentForce = minForce;
    }
    public void ThrowObject()
    {
        Rigidbody rbCube = grabbedObject.GetComponent<Rigidbody>();
        Debug.Log("Throw");
        if (rbCube != null) 
        {
            charging = false;
            rbCube.useGravity = true;
            grabbedObject.transform.parent = grabbedObject.GetComponent<GrabableObject>().GetBaseParent();
            rbCube.AddForce(transform.forward * currentForce + Vector3.up * currentForce,ForceMode.Impulse);
        }
    }

    private void Charge()
    {
        Debug.Log(currentForce);
        currentForce = Mathf.Lerp(currentForce, maxForce, Time.deltaTime * forceGrow);
        Debug.Log(currentForce);
    }

}

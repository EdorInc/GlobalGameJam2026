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
    private float forceGrow = 10;

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
        transform.position = transform.position - transform.forward * Time.deltaTime;
        if (charging)
        {
            Charge();
        }
    }

    public void GrabObject()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.position + 10 * transform.forward, out hit, mask))
        {
            Debug.DrawLine(transform.position, transform.position + 10 * transform.forward);
            Debug.Log("Hola");
            grabbedObject = hit.transform.gameObject;
            grabbedObject.transform.parent = transform;
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

        if (rbCube != null) 
        {
            charging = false;
            grabbedObject.transform.parent = grabbedObject.GetComponent<GrabableObject>().GetBaseParent();
            rbCube.AddForce(transform.forward * currentForce);
        }
    }

    private void Charge()
    {
        Mathf.Lerp(currentForce, maxForce, Time.deltaTime * forceGrow);
    }

}

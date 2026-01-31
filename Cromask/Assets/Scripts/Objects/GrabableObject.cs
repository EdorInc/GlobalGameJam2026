using UnityEngine;

public class GrabableObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private LayerMask groundLayer; 
    [SerializeField] private float groundCheckDistance = 0.6f; 


    private Transform baseParent;
    private Rigidbody Rigidbody;
    
    private void Awake()
    {
        baseParent = transform.parent;
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.freezeRotation = true;
    }
    private void Update()
    {
        CheckGround();
    }
    public Transform GetBaseParent()
    {
        return baseParent;
    }

    private void CheckGround()
    {
        Debug.DrawRay(transform.position, Vector3.down, Color.red, 5);

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            Rigidbody.isKinematic = true;
        }
        else
        {
            Rigidbody.isKinematic = false;
        }
    }

    public bool IsGrabbed()
    {
        return transform.parent != baseParent;
    }
}

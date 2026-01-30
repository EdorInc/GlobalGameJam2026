using UnityEngine;

public class GrabableObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Transform baseParent;
    private Rigidbody rigidbody;

    private void Awake()
    {
        baseParent = transform.parent;
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.freezeRotation = true;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Transform GetBaseParent()
    {
        return baseParent;
    }
}

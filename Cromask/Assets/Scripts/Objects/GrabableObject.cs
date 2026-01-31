using UnityEngine;

public class GrabableObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Transform baseParent;
    private Rigidbody Rigidbody;

    private void Awake()
    {
        baseParent = transform.parent;
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.freezeRotation = true;
    }
    public Transform GetBaseParent()
    {
        return baseParent;
    }
}

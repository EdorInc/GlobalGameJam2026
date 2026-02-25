using Unity.VisualScripting;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    [Header("Break Settings")]
    [SerializeField] float speedToBreak = 10;

    private Rigidbody rb;
    private float maxSpeed = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //Set speed to know if it should be destroyed
        maxSpeed = Mathf.Max(maxSpeed, rb.linearVelocity.magnitude);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<RigidbodyCharacterController>())
            return;
        Debug.Log(maxSpeed + ":" + speedToBreak);

        if (maxSpeed > speedToBreak)
        {
            Destroy(this.gameObject);
        }
        maxSpeed = 0;

    }
}

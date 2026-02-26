using Unity.VisualScripting;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    [Header("Break Settings")]
    [Tooltip("Speed needed in magnitud of the vector to break the rock on impact")]
    [SerializeField] private float speedToBreak = 10;
    [SerializeField] private GameObject particlePrefab;

    private Rigidbody rb;
    private float maxSpeed = 0;
    private RockSpawner spawner;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetSpawner(RockSpawner spawner)
    {
        this.spawner = spawner;
    }

    void Update()
    {
        //Set speed to know if it should be destroyed
        maxSpeed = Mathf.Max(maxSpeed, rb.linearVelocity.magnitude);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Ignore the player so it doesnt break when thrown 
        if (collision.gameObject.CompareTag("Player"))
            return;
        if (maxSpeed > speedToBreak)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            Instantiate(particlePrefab, transform.position, Quaternion.identity);
            spawner.DestroyRock();
        }
        //Reset speed when hitting with not enough force
        maxSpeed = 0;

    }
}

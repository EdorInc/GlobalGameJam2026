using Hanzzz.MeshDemolisher;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    [Header("Break Settings")]
    [Tooltip("Speed needed in magnitud of the vector to break the rock on impact")]
    [SerializeField] private float speedToBreak = 10;
    [SerializeField] private bool useParticles = true;
    [SerializeField] private GameObject particlePrefab;

    [Header("Fragment Settings")]
    [SerializeField] private bool useFragment = true;
    [SerializeField] private GameObject intactMesh;
    [SerializeField] private GameObject fragmentParent;
    [Tooltip("Amount of seconds it takes for the fragments to disappear")]
    [SerializeField] private float fragmentLifespan = 2.0f;
    [SerializeField] private float fragmentForceFromCenter = 10f;
    [SerializeField] private float fragmentForceFromCollision = 5f;

    private Rigidbody rb;
    private float maxSpeed = 0;
    private RockSpawner spawner;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Ensure correct initial state
        if (intactMesh != null) intactMesh.SetActive(true);
        if (fragmentParent != null) fragmentParent.SetActive(false);
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
            if(useParticles) Instantiate(particlePrefab, transform.position, Quaternion.identity);
            Break(collision);
        }

        //Reset speed when hitting with not enough force
        maxSpeed = 0;
    }

    private void Break(Collision collision)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (particlePrefab != null)
            Instantiate(particlePrefab, transform.position, Quaternion.identity);

        // Switch meshes
        if (intactMesh != null) intactMesh.SetActive(false);
        if (fragmentParent != null) fragmentParent.SetActive(true);

        // Disable this collider so it doesn't interfere
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Apply forces to fragments
        if (useFragment)
        {
            foreach (Transform fragment in fragmentParent.transform)
            {
                Rigidbody fragRb = fragment.GetComponent<Rigidbody>();
                if (fragRb == null) continue;

                // Force from center
                fragRb.AddExplosionForce(
                    fragmentForceFromCenter,
                    transform.position,
                    5f
                );

                // Force from collision point
                Vector3 collisionPoint = collision.contacts[0].point;
                Vector3 dir = (fragment.position - collisionPoint).normalized;

                fragRb.AddForce(dir * fragmentForceFromCollision, ForceMode.Impulse);
            }

            if (spawner != null)
                spawner.DestroyRock(fragmentLifespan);
            else Destroy(gameObject, fragmentLifespan);
        }
        else
        {
            if (spawner != null)
                spawner.DestroyRock();
            else Destroy(gameObject);

        }
    }
}

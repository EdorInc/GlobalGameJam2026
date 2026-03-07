using UnityEngine;

public class AirCurrents : MonoBehaviour
{

    [Header("Force Settings")]
    [SerializeField] private Vector3 force = Vector3.zero;


    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(force);
        }
    }
}

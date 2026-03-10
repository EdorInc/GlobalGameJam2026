using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
       
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            EventManager.OnDamageRecived?.Invoke(collision.gameObject, collision.contacts[0].point);
        }
    }
}

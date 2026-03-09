using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EventManager.OnDamageRecived?.Invoke(other.gameObject);
        }
    }
}

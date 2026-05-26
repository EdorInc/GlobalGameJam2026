using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        throw new System.NotImplementedException("The collider must not be a trigger!");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            EventManager.OnDamageRecived?.Invoke(collision.gameObject, collision.contacts[0].point);
            AudioSystem.PlaySFX(AudioSystem.SoundLibrary.hurtSound, collision.gameObject.transform.position);
        }
    }
}

using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Connection Settings")]
    [SerializeField] private int channel = 1;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rock"))
        {
            EventManager.OnButtonPressed?.Invoke(channel);
        }
    }
}

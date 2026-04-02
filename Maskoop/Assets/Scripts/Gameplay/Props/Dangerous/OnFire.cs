using UnityEngine;

public class OnFire : MonoBehaviour
{
    [Header("Is On Fire Settings")]
    [SerializeField] private bool IsOnFire;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && IsOnFire)
        {
            EventManager.OnLitOnFire?.Invoke(other);
        }
    }
}

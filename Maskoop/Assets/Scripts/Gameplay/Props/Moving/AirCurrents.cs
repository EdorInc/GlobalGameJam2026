using UnityEngine;

public class AirCurrents : MonoBehaviour
{

    [Header("Force Settings")]
    [SerializeField] private Vector3 force = Vector3.zero;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.OnAirCurrentEnter?.Invoke(other,force);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.OnAirCurrentExit?.Invoke(other);
        }
    }
}

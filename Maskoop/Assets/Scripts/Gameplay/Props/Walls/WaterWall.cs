using UnityEngine;

public class WaterWall : MonoBehaviour
{
    [SerializeField] private float slowMultiplier = 0.4f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.OnWaterWallEnter?.Invoke(other, slowMultiplier);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.OnWaterWallExit?.Invoke(other);
        }
    }

    
}
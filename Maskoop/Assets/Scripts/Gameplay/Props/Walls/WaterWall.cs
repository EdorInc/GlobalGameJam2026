using UnityEngine;

public class WaterWall : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            EventManager.OnWaterWall?.Invoke(collision,transform);
        }
    }
}

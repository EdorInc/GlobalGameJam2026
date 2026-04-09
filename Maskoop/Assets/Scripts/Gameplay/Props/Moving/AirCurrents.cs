using UnityEngine;

public class AirCurrents : MonoBehaviour
{

    [Header("Force Settings")]
    [SerializeField] private Vector3 force = Vector3.zero;
    [SerializeField] private bool isVertical = false;

    private BoxCollider trigger;
    [SerializeField] private ParticleSystem ps;

    private ParticleSystem.ShapeModule shape;

    private void Start()
    {
        trigger = GetComponent<BoxCollider>();
    }

    private void OnEnable()
    {
        if (ps != null)
            shape = ps.shape;
    }

    private void Update()
    {
        if (trigger == null || ps == null) return;

        Vector3 size = trigger.size;
        Vector3 lossyScale = trigger.transform.lossyScale;

        // Final world size of the collider
        Vector3 worldSize = Vector3.Scale(size, lossyScale);

        // Apply to particle shape
        
        if (isVertical)
        {
            shape.scale = new Vector3(worldSize.z, worldSize.x, worldSize.y);
            shape.rotation = new Vector3(270, 0, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.OnAirCurrentEnter?.Invoke(other,force,isVertical);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.OnAirCurrentExit?.Invoke(other,isVertical);
        }
    }
}

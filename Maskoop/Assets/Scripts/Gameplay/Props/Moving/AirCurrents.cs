using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AirCurrents : MonoBehaviour
{
    [Header("Force Settings")]
    [SerializeField] private float force = 50.0f;

    [Header("Particle Settings")]
    private BoxCollider trigger;
    [SerializeField] private ParticleSystem particles;

    private float m_length = 1.0f;
    private float m_headSize = 0.2f;

    private ParticleSystem.ShapeModule particlesShape;

    private void Start()
    {
        trigger = GetComponent<BoxCollider>();
    }

    void OnDrawGizmos()
    {
        Vector3 arrowHead;
        Vector3 arrowRim;
        Gizmos.color = Color.red;

        arrowHead = transform.position + m_length * transform.up;
        arrowRim = transform.position + m_length * 0.66f * transform.up;

        Gizmos.DrawLine(transform.position, arrowHead);
        Gizmos.DrawLineStrip(new Vector3[3] { arrowRim + transform.right * m_length * m_headSize, arrowHead, arrowRim - transform.right * m_length * m_headSize }, true);
        Gizmos.DrawLineStrip(new Vector3[3] { arrowRim + transform.forward * m_length * m_headSize, arrowHead, arrowRim - transform.forward * m_length * m_headSize }, true);
    }

    private void OnEnable()
    {
        if (particles != null)
        {
            particlesShape = particles.shape;
        }
    }

    private void Update()
    {
        if (trigger == null || particles == null) return;

        Vector3 size = trigger.size;
        Vector3 lossyScale = trigger.transform.lossyScale;

        // Final world size of the collider
        Vector3 worldSize = Vector3.Scale(size, lossyScale);

        // Apply to particle shape
        particles.transform.position = trigger.bounds.center;
        particlesShape.scale = new Vector3(worldSize.x, worldSize.z, worldSize.y);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 forceVector = transform.up * force;
            EventManager.OnAirCurrentEnter?.Invoke(other, forceVector);
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

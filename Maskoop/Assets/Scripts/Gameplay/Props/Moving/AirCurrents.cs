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
        Gizmos.color = Color.red;

        // Use the parent's pivot when available, otherwise use this transform.
        Transform pivot = transform.parent != null ? transform.parent : transform;
        Vector3 origin = pivot.position;

        Vector3 arrowHead = origin + pivot.up * m_length;
        Vector3 arrowRim = origin + pivot.up * (m_length * 0.66f);

        Vector3 rightOffset = pivot.right * (m_length * m_headSize);
        Vector3 forwardOffset = pivot.forward * (m_length * m_headSize);

        // Shaft
        Gizmos.DrawLine(origin, arrowHead);

        // Head rim (right/left)
        Gizmos.DrawLine(arrowRim + rightOffset, arrowHead);
        Gizmos.DrawLine(arrowHead, arrowRim - rightOffset);

        // Head rim (forward/back)
        Gizmos.DrawLine(arrowRim + forwardOffset, arrowHead);
        Gizmos.DrawLine(arrowHead, arrowRim - forwardOffset);
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

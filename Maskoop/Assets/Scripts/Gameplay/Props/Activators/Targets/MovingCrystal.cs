using UnityEngine;

public class MovingCrystal : MonoBehaviour
{
    [Header("Floating")]
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatSpeed = 1f;

    [Header("Rotation")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 50f, 0f);

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Movimiento arriba y abajo
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = startPosition + Vector3.up * offset;

        // Rotación sobre sí mismo
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
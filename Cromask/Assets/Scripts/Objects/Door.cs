using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private PressurePlate[] requiredPlates;
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 closedPosition;
    private Vector3 openedPosition;
    private bool isDoorOpened = false;
    private int activatedPlatesCount = 0;

    void Start()
    {
        float openedHeight = transform.localScale.y;
        closedPosition = transform.position;
        openedPosition = closedPosition - Vector3.up * openedHeight;

        foreach (PressurePlate plate in requiredPlates)
        {
            if (plate != null)
            {
                // Aquí necesitamos que PressurePlate tenga eventos públicos
                plate.onPlateActivated.AddListener(OnPlateActivated);
            }
            else
            {
                Debug.LogWarning("Una de las placas asignadas a la puerta es null");
            }
        }
    }

    void Update()
    {
        Vector3 targetPosition = isDoorOpened ? openedPosition : closedPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    private void OnPlateActivated()
    {
        activatedPlatesCount++;

        Debug.Log($"Placa activada. Total: {activatedPlatesCount}/{requiredPlates.Length}");

        if (activatedPlatesCount >= requiredPlates.Length)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        if (!isDoorOpened)
        {
            isDoorOpened = true;
            Debug.Log("¡Puerta abierta! Todas las placas activadas.");
        }
    }

    private void OnDestroy()
    {
        foreach (PressurePlate plate in requiredPlates)
        {
            if (plate != null)
            {
                plate.onPlateActivated.RemoveListener(OnPlateActivated);
            }
        }
    }
}

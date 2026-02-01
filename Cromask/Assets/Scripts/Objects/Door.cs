using FMOD;
using FMODUnity;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private PressurePlate[] requiredPlates;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float openDelay = 1f;


    private Vector3 closedPosition;
    private Vector3 openedPosition;
    private bool isDoorOpened = false;
    private int activatedPlatesCount = 0;
    private Light[] lights;
    private float doorOpenDuration;

    void Start()
    {
        float openedHeight = transform.localScale.y;
        closedPosition = transform.position;
        openedPosition = closedPosition - Vector3.up * openedHeight;

        doorOpenDuration = openedHeight / moveSpeed;

        lights = GetComponentsInChildren<Light>();

        foreach (PressurePlate plate in requiredPlates)
        {
            if (plate != null)
            {
                // Aquí necesitamos que PressurePlate tenga eventos públicos
                plate.onPlateActivated.AddListener(OnPlateActivated);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Una de las placas asignadas a la puerta es null");
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

        UnityEngine.Debug.Log($"Placa activada. Total: {activatedPlatesCount}/{requiredPlates.Length}");

        foreach (Light light in lights)
        {
            if (light.enabled == false)
            {
                light.enabled = true;
                break;
            }
        }   

        PlayerCameraInterface player1Camera = ReferenceManager.Instance.GetPlayerOneCamera().GetComponent<PlayerCameraInterface>();
        PlayerCameraInterface player2Camera = ReferenceManager.Instance.GetPlayerTwoCamera().GetComponent<PlayerCameraInterface>();
        float focusDuration = activatedPlatesCount >= requiredPlates.Length ? openDelay + doorOpenDuration : openDelay;
        
        if (player1Camera != null) player1Camera.FocusOnTarget(transform, focusDuration);
        if (player2Camera != null) player2Camera.FocusOnTarget(transform, focusDuration);

        if (activatedPlatesCount >= requiredPlates.Length)
        {
            Invoke("OpenDoor", openDelay);
        }
    }

    private void OpenDoor()
    {
        if (!isDoorOpened)
        {
            isDoorOpened = true;

            ATTRIBUTES_3D attr = new ATTRIBUTES_3D();

            attr.position = RuntimeUtils.ToFMODVector(transform.position);
            attr.forward = RuntimeUtils.ToFMODVector(transform.forward);
            attr.up = RuntimeUtils.ToFMODVector(transform.up);

            AudioManager.Instance.PlaySFX(AudioType.Door, attr);

            UnityEngine.Debug.Log("¡Puerta abierta! Todas las placas activadas.");
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

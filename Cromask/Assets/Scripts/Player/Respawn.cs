using FMOD;
using FMODUnity;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("Margen para verificar suelo en todas direcciones")]
    [SerializeField] 
    private float groundCheckMargin = 2f;
    [Tooltip("Número de raycasts por dirección")]
    [SerializeField] 
    private int numRayChecks = 4;
    [Tooltip("Capa del suelo")]
    [SerializeField] 
    private LayerMask groundLayer;
    [Tooltip("Distancia máxima del raycast hacia abajo")]
    [SerializeField] 
    private float rayDistance = 5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;

    private CharacterController characterController;
    private Vector3 lastValidPosition;
    private bool hasValidSpawn = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            UnityEngine.Debug.LogError("No se encontró CharacterController en el jugador");
        }
    }

    void Update()
    {
        if (IsGroundAppropriate(transform.position))
        {
            lastValidPosition = transform.position;
            hasValidSpawn = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZzzzone"))
        {
            GameObject grabbedObject = transform.GetComponent<GrabAction>().ThrowObject();

            grabbedObject?.GetComponent<Reset>()?.ForceRespawn();
            grabbedObject?.GetComponent<ResetMask>()?.RespawnObject();

            RespawnPlayer();
        }
    }

    public void ForceRespawn(Transform objTransform)
    {
        hasValidSpawn = true;
        lastValidPosition = objTransform.position + Vector3.up * 1.0f;
        RespawnPlayer();
    }

    private void RespawnPlayer()
    {
        if (hasValidSpawn)
        {
            transform.GetComponent<GrabAction>().ThrowObject();

            if (characterController != null) characterController.enabled = false;
            transform.position = lastValidPosition + Vector3.up * 1.0f; // Elevar un poco para evitar quedar atrapado en el suelo
            if (characterController != null) characterController.enabled = true; // Reactivar

            ATTRIBUTES_3D attr = new ATTRIBUTES_3D();

            attr.position = RuntimeUtils.ToFMODVector(transform.position);
            attr.forward = RuntimeUtils.ToFMODVector(transform.forward);
            attr.up = RuntimeUtils.ToFMODVector(transform.up);

            AudioManager.Instance.PlaySFX(AudioType.Respawn, attr);

            UnityEngine.Debug.Log("Jugador respawneado en la última posición válida");
        }
        else
        {
            UnityEngine.Debug.LogWarning("No hay una posición de respawn válida disponible");
        }
    }

    private bool IsGroundAppropriate(Vector3 position)
    {
        if (!Physics.Raycast(position, Vector3.down, rayDistance, groundLayer))
        {
            UnityEngine.Debug.Log("No hay suelo debajo del jugador");
            return false;
        }

        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        foreach (Vector3 direction in directions)
        {
            if (!CheckGroundInDirection(position, direction))
            {
                return false;
            }
        }

        Vector3[] diagonals = new Vector3[]
        {
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized
        };

        foreach (Vector3 diagonal in diagonals)
        {
            if (!CheckGroundInDirection(position, diagonal))
            {
                return false;
            }
        }

        return true;
    }

    private bool CheckGroundInDirection(Vector3 origin, Vector3 direction)
    {
        for (int i = 1; i <= numRayChecks; i++)
        {
            float distance = (groundCheckMargin / numRayChecks) * i;
            Vector3 checkPosition = origin + direction * distance;

            Ray ray = new Ray(checkPosition, Vector3.down);
            if (showDebugRays)
            {
                UnityEngine.Debug.DrawRay(checkPosition, Vector3.down * rayDistance, Color.cyan, 0.1f);
            }

            if (!Physics.Raycast(ray, rayDistance, groundLayer))
            {
                return false; // No hay suelo en esta dirección
            }
        }

        return true;
    }

    void OnDrawGizmosSelected()
    {
        if (hasValidSpawn)
        {
            // Dibujar el último punto de respawn válido
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastValidPosition, 0.5f);

            // Dibujar el área de margen
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(lastValidPosition, new Vector3(groundCheckMargin * 2, 0.1f, groundCheckMargin * 2));
        }

        // Visualizar el área de comprobación actual
        if (Application.isPlaying)
        {
            Gizmos.color = IsGroundAppropriate(transform.position) ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(groundCheckMargin * 2, 0.1f, groundCheckMargin * 2));
        }
    }
}

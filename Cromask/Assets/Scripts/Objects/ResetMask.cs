using UnityEngine;

public class ResetMask : MonoBehaviour
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

    [SerializeField]
    private float respawnHeight = 10.0f;

    private Vector3 firstValidPosition;
    private Vector3 lastValidPosition;
    private bool hasValidSpawn = false;

    void Start()
    {
        firstValidPosition = transform.position + Vector3.up * respawnHeight;
        lastValidPosition = firstValidPosition;
        hasValidSpawn = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZzzzone"))
        {
            RespawnObject();
        }
    }

    private void RespawnObject()
    {
        if (hasValidSpawn)
        {
            transform.position = lastValidPosition;
            Debug.Log("Objeto respawneado en la última posición válida.");
        }
        else
        {
            transform.position = firstValidPosition;
            Debug.LogWarning("No hay una posición de respawn válida disponible, se respawnea en el origen.");
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

    private bool IsGroundAppropriate(Vector3 position)
    {
        if (!Physics.Raycast(position, Vector3.down, rayDistance, groundLayer))
        {
            Debug.Log("No hay suelo debajo del jugador.");
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

            if (!Physics.Raycast(ray, rayDistance, groundLayer))
            {
                return false; 
            }
        }

        return true;
    }
}

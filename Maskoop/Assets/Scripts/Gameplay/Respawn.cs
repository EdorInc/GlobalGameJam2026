using UnityEngine;

public class Respawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Vector3 respawnPosition;
    public float voidDistance = -3;
    public bool willDestroy = false;

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

    new private Rigidbody rigidbody;
    private BaseSpawner spawner;
    public Quaternion respawnRotation;

    private GroundDetector groundDetector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        respawnRotation = Quaternion.identity;
        groundDetector = GetComponent<GroundDetector>();
    }

    public void SetSpawner(BaseSpawner spawner)
    {
        this.spawner = spawner;
    }

    // Update is called once per frame
    void Update()
    {
        if (rigidbody.position.y < voidDistance)
        {
            if (willDestroy == false)
            {
                RespawnFunction();
            }
            else if (willDestroy == true && spawner != null)
            {
                spawner.DestroyObject();
            } 
            else if(willDestroy == true)
            {
                Destroy();
            }
        }

        if (IsGroundAppropriate(transform.position) && this.gameObject.CompareTag("Player"))
        {
            respawnPosition = transform.position;
        }

    }

    public void RespawnFunction()
    {
        rigidbody.position = respawnPosition;
        if(respawnRotation != Quaternion.identity)
        {
            rigidbody.rotation = respawnRotation;
            rigidbody.linearVelocity = Vector3.zero;
        }
    }

    void Destroy()
    {
        Destroy(gameObject);
    }

    private bool IsGroundAppropriate(Vector3 position)
    {
        if(groundDetector == null)
        {
            return false;
        }
        if (!groundDetector.IsGrounded)
        {
            return false;
        }
        if (!Physics.Raycast(position, Vector3.down, rayDistance, groundLayer))
        {
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
                Debug.DrawRay(checkPosition, Vector3.down * rayDistance, Color.cyan, 0.1f);
            }

            if (!Physics.Raycast(ray, rayDistance, groundLayer))
            {
                return false; // No hay suelo en esta dirección
            }
        }

        return true;
    }
}

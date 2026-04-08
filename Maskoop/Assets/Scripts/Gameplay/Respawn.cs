using UnityEngine;

public class Respawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("Position where the object will respawn after falling below the void distance.")]
    public Vector3 respawnPosition;
    [Tooltip("Height threshold below which the object will respawn or be destroyed.")]
    public float voidDistance = -3;
    [Tooltip("If true, the object will be destroyed instead of respawned when falling below the void distance.")]
    public bool willDestroy = false;


    [Header("Ground Settings")]
    [Tooltip("Margin used to check for ground in all directions around the object.")]
    [SerializeField]
    private float groundCheckMargin = 2f;
    [Tooltip("Number of raycasts to perform in each direction for ground checking.")]
    [SerializeField]
    private int numRayChecks = 4;
    [Tooltip("Layer mask used to identify ground surfaces.")]
    [SerializeField]
    private LayerMask groundLayer;
    [Tooltip("Maximum distance for downward raycasts when checking for ground.")]
    [SerializeField]
    private float rayDistance = 5f;
    [Tooltip("The area to check if player is ground appropriate")]
    [SerializeField]
    private Vector3[] areaCheck;

    [Header("Debug Settngs")]
    [Tooltip("Show debug rays in the scene for ground checking.")]
    [SerializeField] private bool showDebugRays = true;
    [Tooltip("If true, a visual marker for the respawn position will be created in the scene.")]
    [SerializeField] private bool createVisualRespawnPosition = false;

    private Transform visualRespawnPositon;

    private Rigidbody rigidBody;
    private BaseSpawner spawner;

    [HideInInspector]
    public Quaternion respawnRotation;

    private GroundDetector groundDetector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        respawnRotation = Quaternion.identity;
        groundDetector = GetComponent<GroundDetector>();
    }

    void Start()
    {
        // Create a new game object to visualize the respawn position if enabled
        if (createVisualRespawnPosition && visualRespawnPositon == null)
        {
            string parentName = gameObject.name;
            GameObject visualRespawnGO = new GameObject($"{parentName}_VisualRespawnPosition");
            visualRespawnGO.transform.position = respawnPosition;
            visualRespawnPositon = visualRespawnGO.transform;
        }
    }

    public void SetSpawner(BaseSpawner spawner)
    {
        this.spawner = spawner;
    }

    /// <summary>
    /// Updates the respawn position and moves the visual respawn marker to the new position.
    /// </summary>
    /// <param name="newPosition">The new respawn position.</param>
    public void UpdateRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
        if (visualRespawnPositon != null)
        {
            visualRespawnPositon.position = newPosition;
        }
    }

    void Update()
    {
        if (rigidBody.position.y < voidDistance)
        {
            if (willDestroy == false)
            {
                RespawnFunction();
            }
            else
            {
                if (spawner != null)
                {
                    spawner.DestroyObject();
                }
                else
                {
                    DestroyFunction();
                }
            } 
        }

        if (IsGroundAppropriate(transform.position) && this.gameObject.CompareTag("Player"))
        {
            UpdateRespawnPosition(transform.position);
        }

    }

    public void RespawnFunction()
    {
        rigidBody.position = respawnPosition;
        if(respawnRotation != Quaternion.identity)
        {
            rigidBody.rotation = respawnRotation;
            rigidBody.linearVelocity = Vector3.zero;
        }
    }

    void DestroyFunction()
    {
        Destroy(gameObject);
    }

    // TODO Find the farthest point from the edge not just whether you are on perfect ground.
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

        foreach (Vector3 direction in areaCheck)
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

            // No ground detected in this direction, return false
            if (!Physics.Raycast(ray, rayDistance, groundLayer))
            {
                return false;
            }
        }

        return true;
    }
}

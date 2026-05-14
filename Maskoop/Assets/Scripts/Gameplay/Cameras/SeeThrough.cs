using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SeeThrough : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform player;
    [SerializeField]
    private Transform otherPlayer;

    private Camera cam;

    [Header("Cast Settings")]
    public float castSphereRadius = 0.5f;
    public bool useNearSphereCast = false;
    public float nearSphereDistance = 1.5f;

    [Header("Wall Settings")]
    public string wallTag = "Wall";
    public LayerMask raycastMask;
    public int seeThroughLayer;

    // Keeps track of walls currently made see-through
    [SerializeField]
    private readonly Dictionary<GameObject, int> modifiedWalls = new Dictionary<GameObject, int>();

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("SeeThrough script must be attached to a Camera.");
            enabled = false;
        }
    }

    void Update()
    {
        if (!enabled || cam == null || player == null) return;

        Vector3 origin = cam.transform.position;
        HashSet<GameObject> wallsThisFrame = new HashSet<GameObject>();

        // We always process the main player
        ProcessTarget(origin, player, wallsThisFrame);

        // In merged state, also process the other player
        if (otherPlayer != null)
        {
            ProcessTarget(origin, otherPlayer, wallsThisFrame);
        }

        // -------- Restore walls --------

        List<GameObject> toRestore = new List<GameObject>();

        foreach (var pair in modifiedWalls)
        {
            GameObject wall = pair.Key;

            if (wall == null || !wallsThisFrame.Contains(wall))
            {
                if (wall != null)
                {
                    wall.layer = pair.Value;
                }

                toRestore.Add(wall);
            }
        }

        foreach (GameObject obj in toRestore)
        {
            SetLayerRecursively(obj, modifiedWalls[obj]);
            modifiedWalls.Remove(obj);
        }

        // -------- Apply see-through --------

        foreach (GameObject obj in wallsThisFrame)
        {
            if (!modifiedWalls.ContainsKey(obj))
            {
                modifiedWalls[obj] = obj.layer;
                SetLayerRecursively(obj, seeThroughLayer);
            }
        }
    }

    private void ProcessTarget(Vector3 origin, Transform targetTransform, HashSet<GameObject> wallsThisFrame)
    {
        if (targetTransform == null)
        {
            return;
        }

        Vector3 target = targetTransform.position;
        Vector3 direction = target - origin;

        float distance = direction.magnitude;

        if (distance <= Mathf.Epsilon)
        {
            return;
        }

        // Reuse the already computed distance to avoid an extra normalization cost.
        Vector3 dirNorm = direction / distance;

        Debug.DrawLine(origin, target, Color.red);

        float effectiveNearDistance = Mathf.Min(nearSphereDistance, distance);

        // -------- SphereCast near camera --------

        if (useNearSphereCast && effectiveNearDistance > 0f)
        {
            RaycastHit[] nearHits = Physics.SphereCastAll(
                new Ray(origin, dirNorm),
                castSphereRadius,
                effectiveNearDistance,
                raycastMask
            );

            foreach (RaycastHit hit in nearHits)
            {
                TryAddWall(hit.collider, wallsThisFrame);
            }
        }

        // -------- Raycast for the rest --------

        float startOffset = useNearSphereCast ? effectiveNearDistance : 0f;
        float remainingDistance = distance - startOffset;

        if (remainingDistance <= 0f)
        {
            return;
        }

        Vector3 rayStart = origin + dirNorm * startOffset;

        // Better than Linecast because it returns all blockers on the segment, not only the first one, so multiple walls can be handled in a single frame.
        RaycastHit[] hits = Physics.RaycastAll(
            new Ray(rayStart, dirNorm),
            remainingDistance,
            raycastMask
        );

        foreach (RaycastHit hit in hits)
        {
            TryAddWall(hit.collider, wallsThisFrame);
        }
    }

    public void SetPlayers(Transform mainPlayer, Transform secondaryPlayer)
    {
        player = mainPlayer;
        otherPlayer = secondaryPlayer;
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            Debug.Log("Child found: " + child.name);
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void TryAddWall(Collider collider, HashSet<GameObject> walls)
    {
        if (collider == null)
        {
            return;
        }

        GameObject obj = collider.gameObject;

        if (!obj.CompareTag(wallTag))
        {
            return;
        }

        // Climb up the hierarchy to find the highest parent that is also tagged as a Wall
        Transform current = obj.transform;
        Transform highestWall = current;

        while (current.parent != null)
        {
            current = current.parent;
            if (current.CompareTag(wallTag))
            {
                highestWall = current;
            }
        }

        walls.Add(highestWall.gameObject);
    }


}

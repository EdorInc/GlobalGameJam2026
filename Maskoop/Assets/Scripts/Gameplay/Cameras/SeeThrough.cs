using System.Collections.Generic;
using UnityEngine;

public class SeeThrough : MonoBehaviour
{
    [Header("References")]
    public Transform player;

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
    private Dictionary<GameObject, int> modifiedWalls = new Dictionary<GameObject, int>();

    void Start()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("SeeThrough script must be attached to a Camera.");
            enabled = false;
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player Transform reference is missing in SeeThrough script.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        Vector3 origin = cam.transform.position;
        Vector3 target = player.position;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;
        Vector3 dirNorm = direction.normalized;

        Debug.DrawLine(origin, target, Color.red);

        HashSet<GameObject> wallsThisFrame = new HashSet<GameObject>();

        nearSphereDistance = Mathf.Min(nearSphereDistance, distance);

        // -------- SphereCast near camera --------
        Ray ray = new Ray(origin, dirNorm);

        if (useNearSphereCast)
        {
            RaycastHit[] nearHits = Physics.SphereCastAll(
                ray,
                castSphereRadius,
                nearSphereDistance,
                raycastMask
            );

            foreach (RaycastHit hit in nearHits)
            {
                TryAddWall(hit.collider, wallsThisFrame);
            }
        }

        // -------- Linecast for the rest --------
        Vector3 lineStart = origin;
        if (useNearSphereCast)  lineStart = origin + dirNorm * nearSphereDistance;

        if (Physics.Linecast(lineStart, target, out RaycastHit lineHit, raycastMask))
        {
            TryAddWall(lineHit.collider, wallsThisFrame);
        }

        // -------- Apply see-through --------
        foreach (GameObject obj in wallsThisFrame)
        {
            if (!modifiedWalls.ContainsKey(obj))
            {
                modifiedWalls[obj] = obj.layer;
                obj.layer = seeThroughLayer;
            }
        }

        // -------- Restore walls --------
        List<GameObject> toRestore = new List<GameObject>();

        foreach (var pair in modifiedWalls)
        {
            GameObject wall = pair.Key;

            if (wall == null || !wallsThisFrame.Contains(wall))
            {
                if (wall != null)
                    wall.layer = pair.Value;

                toRestore.Add(wall);
            }
        }

        foreach (GameObject obj in toRestore)
        {
            modifiedWalls.Remove(obj);
        }
    }

    void TryAddWall(Collider collider, HashSet<GameObject> walls)
    {
        if (collider == null)
            return;

        GameObject obj = collider.gameObject;

        if (!obj.CompareTag(wallTag))
            return;

        walls.Add(obj);
    }


}

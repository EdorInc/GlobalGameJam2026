using System.Collections.Generic;
using UnityEngine;

public class PipeNode : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject straightPrefab;
    [SerializeField] private GameObject cornerPrefab;
    [SerializeField] private GameObject endPrefab;

    [Header("Settings")]
    [SerializeField] private LayerMask pipeLayer;
    [SerializeField] private float detectionRadius = 0.2f;

    private bool north;
    private bool south;
    private bool east;
    private bool west;
    private bool up;
    private bool down;

    [ContextMenu("Refresh Pipe")]
    public void Refresh()
    {
        DetectConnections();

        foreach (Transform child in transform)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
#endif
                Destroy(child.gameObject);
        }

        SpawnCorrectPiece();
    }

    void DetectConnections()
    {
        north = HasPipe(Vector3.forward);
        south = HasPipe(Vector3.back);
        east = HasPipe(Vector3.right);
        west = HasPipe(Vector3.left);
        up = HasPipe(Vector3.up);
        down = HasPipe(Vector3.down);
    }

    bool HasPipe(Vector3 dir)
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position + dir,
            detectionRadius,
            pipeLayer
        );

        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject)
                return true;
        }

        return false;
    }

    void SpawnCorrectPiece()
    {
        int horizontalConnections = CountHorizontalConnections();

        if (horizontalConnections == 1 || horizontalConnections == 2 && (up ||down))
        {

            if (up)
            {
                if (south)
                    Spawn(cornerPrefab, Quaternion.Euler(270, 90, 0));
                else if (north)
                    Spawn(cornerPrefab, Quaternion.Euler(270, 270, 0));
                else if (east)
                    Spawn(cornerPrefab, Quaternion.Euler(270, 0, 0));
                else if (west)
                    Spawn(cornerPrefab, Quaternion.Euler(270, 180, 0));

                return;
            }

            if (down)
            {
                if (north)
                    Spawn(cornerPrefab, Quaternion.Euler(90, 270, 0));

                else if (east)
                    Spawn(cornerPrefab, Quaternion.Euler(90, 0, 0));

                else if (south)
                    Spawn(cornerPrefab, Quaternion.Euler(90, 90, 0));

                else if (west)
                    Spawn(cornerPrefab, Quaternion.Euler(90, 180, 0));

                return;
            }

            if (north)
                Spawn(endPrefab, Quaternion.Euler(90, 180, 0));

            else if (east)
                Spawn(endPrefab, Quaternion.Euler(90, 270, 0));

            else if (south)
                Spawn(endPrefab, Quaternion.Euler(90, 0, 0));

            else if (west)
                Spawn(endPrefab, Quaternion.Euler(90, 90, 0));

            return;
        }

        if ((north && south) || (east && west))
        {
            Quaternion rot = Quaternion.identity;

            if (east && west)
                rot = Quaternion.Euler(0, 90, 0);

            Spawn(straightPrefab, rot);
            return;
        }

 
        if (horizontalConnections == 2)
        {
            if (north && east)
                Spawn(cornerPrefab, Quaternion.identity);

            else if (east && south)
                Spawn(cornerPrefab, Quaternion.Euler(0, 90, 0));

            else if (south && west)
                Spawn(cornerPrefab, Quaternion.Euler(0, 180, 0));

            else if (west && north)
                Spawn(cornerPrefab, Quaternion.Euler(0, 270, 0));

            return;
        }


        if (up || down)
        {
            Spawn(straightPrefab, Quaternion.Euler(90, 0, 0));
            return;
        }
    }

    int CountHorizontalConnections()
    {
        int count = 0;

        if (north) count++;
        if (south) count++;
        if (east) count++;
        if (west) count++;

        return count;
    }

    void Spawn(GameObject prefab, Quaternion rot)
    {
        Instantiate(
            prefab,
            transform.position,
            rot,
            transform
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        DrawCheck(Vector3.forward);
        DrawCheck(Vector3.back);
        DrawCheck(Vector3.right);
        DrawCheck(Vector3.left);
        DrawCheck(Vector3.up);
        DrawCheck(Vector3.down);
    }

    void DrawCheck(Vector3 dir)
    {
        Gizmos.DrawWireSphere(
            transform.position + dir,
            detectionRadius
        );
    }
}
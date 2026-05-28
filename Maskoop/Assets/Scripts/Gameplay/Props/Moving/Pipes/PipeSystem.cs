using System.Collections.Generic;
using UnityEngine;

public class PipeSystem : MonoBehaviour
{
    [SerializeField] private float connectionDistance = 1.1f;

    public List<PipeNode> orderedPath = new();

    [ContextMenu("Generate Path")]
    public void GeneratePath()
    {
        orderedPath.Clear();

        PipeNode[] pipes = GetComponentsInChildren<PipeNode>();

        if (pipes.Length == 0)
            return;

        // Buscar endpoints
        List<PipeNode> ends = new();

        foreach (var pipe in pipes)
        {
            int connections = GetConnections(pipe, pipes).Count;

            if (connections == 1)
                ends.Add(pipe);
        }

        if (ends.Count == 0)
        {
            Debug.LogWarning("No endpoints found.");
            return;
        }

        PipeNode start = ends[0];

        HashSet<PipeNode> visited = new();

        Traverse(start, null, pipes, visited);
    }

    void Traverse(
        PipeNode current,
        PipeNode previous,
        PipeNode[] pipes,
        HashSet<PipeNode> visited)
    {
        if (visited.Contains(current))
            return;

        visited.Add(current);

        orderedPath.Add(current);

        List<PipeNode> neighbors = GetConnections(current, pipes);

        foreach (var neighbor in neighbors)
        {
            if (neighbor == previous)
                continue;

            Traverse(neighbor, current, pipes, visited);
        }
    }

    List<PipeNode> GetConnections(PipeNode pipe, PipeNode[] allPipes)
    {
        List<PipeNode> result = new();

        foreach (var other in allPipes)
        {
            if (other == pipe)
                continue;

            float dist = Vector3.Distance(
                pipe.transform.position,
                other.transform.position
            );

            if (dist <= connectionDistance)
            {
                Vector3 dir =
                    other.transform.position -
                    pipe.transform.position;

                dir.x = Mathf.Round(dir.x);
                dir.y = Mathf.Round(dir.y);
                dir.z = Mathf.Round(dir.z);

                if (
                    dir == Vector3.forward ||
                    dir == Vector3.back ||
                    dir == Vector3.right ||
                    dir == Vector3.left ||
                    dir == Vector3.up ||
                    dir == Vector3.down
                )
                {
                    result.Add(other);
                }
            }
        }

        return result;
    }

    public List<Vector3> GetWorldPositions()
    {
        List<Vector3> positions = new();

        foreach (var pipe in orderedPath)
        {
            positions.Add(pipe.transform.position);
        }

        return positions;
    }

    private void OnDrawGizmos()
    {
        if (orderedPath == null || orderedPath.Count < 2)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < orderedPath.Count - 1; i++)
        {
            Gizmos.DrawLine(
                orderedPath[i].transform.position,
                orderedPath[i + 1].transform.position
            );
        }
    }
}
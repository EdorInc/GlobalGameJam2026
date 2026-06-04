using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TransformList
{
    public List<Transform> transforms;
}

[System.Serializable]
public enum TileType{
    Ground,
    Air,
    Wall
}

[System.Serializable]
public class TileData
{
    public Vector3 position;
    public TileType tileType;
    public GameObject occupiedBy;
}

public class NavMeshManager : MonoBehaviour
{
    private List<List<TileData>> tileMatrix;

    private Dictionary<Vector2, TileData> mapDictionary;

    public TileMatrixGenerator tileMatrixGenerator;

    public LayerMask playerLayer;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 0.2f)
        {
            UpdateTileBlocking();
            timer = 0;
        }
    }
    private void Awake()
    {
        tileMatrix = new List<List<TileData>>();
        mapDictionary = new Dictionary<Vector2, TileData>();
        tileMatrixGenerator.GenerateTiles();
        for (int i = 0; i < tileMatrixGenerator.tiles.GetLength(0); i++)
        {
            List<TileData> tiledataAux = new List<TileData>();
            tileMatrix.Add(tiledataAux);
            for (int j = 0; j < tileMatrixGenerator.tiles.GetLength(1); j++)
            {
                TileData data = tileMatrixGenerator.tiles[i, j];
                tiledataAux.Add(data);
                mapDictionary.Add(new Vector2(data.position.x, data.position.z), data);
            }
        }
    }

    public Vector2 WorldToTile(Vector3 worldPosition)
    {
        Vector3 p = worldPosition;

        return new Vector2(
            Mathf.RoundToInt(p.x),
            Mathf.RoundToInt(p.z)
        );
    }

    public List<Vector3> FindPath(Vector2 start, Vector2 goal)
    {
        if (!mapDictionary.ContainsKey(goal))
        {
            return null;
        }
        TileData startData = mapDictionary[start];
        TileData endData = mapDictionary[goal];

        var openSet = new PriorityQueue<TileData>();
        var cameFrom = new Dictionary<TileData, TileData>();
        var gScore = new Dictionary<TileData, float>();
        var fScore = new Dictionary<TileData, float>();

        openSet.Enqueue(startData, 0);
        gScore[startData] = 0;
        fScore[startData] = Heuristic(startData, endData);

        while (openSet.Count > 0)
        {
            TileData current = openSet.Dequeue();

            if (current == endData)
                return ReconstructPath(cameFrom, current);

            foreach (TileData neighbor in GetNeighbors(current,endData))
            {
                float tentativeG = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, endData);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        return null;
    }


    public Vector3 FindNearestEdge(Vector3 targetWorldPos)
    {
        Vector2 fromTile = WorldToTile(targetWorldPos);

        if (!mapDictionary.TryGetValue(fromTile, out TileData startTile))
            return targetWorldPos;

        float bestDist = float.MaxValue;
        TileData bestTile = null;

        foreach (TileData tile in mapDictionary.Values)
        {
            if (tile.tileType != TileType.Ground)
                continue;

            if (IsEdgeTile(tile))
            {
                float dist = Vector3.Distance(startTile.position, tile.position);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestTile = tile;
                }
            }
        }

        return bestTile != null ? bestTile.position : targetWorldPos;
    }

    private bool IsEdgeTile(TileData tile)
    {
        foreach (Vector3 dir in directions)
        {
            Vector2 neighborPos = WorldToTile(tile.position + dir);

            if (!mapDictionary.TryGetValue(neighborPos, out TileData neighbor))
            {
               return true;
            }

            if (neighbor.tileType == TileType.Air)
            {
                return true;
            }
        }

        return false;
    }
    public bool IsTileWalkable(Vector2 tile)
    {
        if(mapDictionary.TryGetValue(tile, out TileData tiledata))
        {
            return tiledata.tileType == TileType.Ground;
        }
        return false;
    }

    private Vector3[] directions = { 
        new Vector3(0,0,1),
        new Vector3(1,0,0),
        new Vector3(0,0,-1),
        new Vector3(-1,0,0),
        new Vector3(1,0,1),
        new Vector3(-1,0,1),
        new Vector3(-1,0,-1),
        new Vector3(1,0,-1),
    };
    private List<TileData> GetNeighbors(TileData current, TileData goal)
    {
        var neighbors = new List<TileData>();

        foreach (Vector3 dir in directions)
        {
            Vector3 neighborPos = current.position + dir;

            if (mapDictionary.TryGetValue(WorldToTile(neighborPos), out TileData neighbor))
            {
                if (neighbor.tileType == TileType.Ground || neighbor == goal)
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    float Heuristic(TileData a, TileData b)
    {
        return Vector3.Distance(a.position, b.position);
    }


    List<Vector3> ReconstructPath(Dictionary<TileData, TileData> cameFrom, TileData current)
    {
        List<Vector3> path = new();

        path.Add(current.position);

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current.position);
        }

        path.Reverse();
        return path;
    }

    //Dinamic tyles
    private void SetTileType(Vector3 worldPos, TileType tileType)
    {
        Vector2 tilePos = WorldToTile(worldPos);

        if(mapDictionary.TryGetValue(tilePos,out TileData data))
        {
            data.tileType = tileType;
        }
    }

    void UpdateTileBlocking()
    {
        bool isChanged = false;
        foreach(TileData tile in mapDictionary.Values)
        {
            bool isBlocked = Physics.CheckBox(tile.position + Vector3.up * 0.6f, new Vector3(0.4f, 0.2f, 0.4f),Quaternion.identity,playerLayer);
            TileType newType = isBlocked ? TileType.Wall : TileType.Ground;
            if(newType != tile.tileType && tile.tileType != TileType.Air)
            {
                SetTileType(tile.position, newType);
                isChanged = true;
            }
        }
        if (isChanged)
        {
            EventManager.OnNavMeshUpdate?.Invoke(this);
        }
    }
    //DEBUGS
    void OnDrawGizmos()
    {
        if (tileMatrix == null)
            return;


        foreach (var row in tileMatrix)
        {
            foreach (var tile in row)
            {
                if (tile == null || tile.position == null) continue;

                switch (tile.tileType)
                {
                    case TileType.Ground:
                        Gizmos.color = Color.green;
                        break;

                    case TileType.Wall:
                        Gizmos.color = Color.red;
                        break;

                    case TileType.Air:
                        Gizmos.color = Color.cyan;
                        break;
                }
                foreach (Vector3 dir in directions)
                {
                    Vector3 neighbor = tile.position + dir;

                    if (mapDictionary != null &&
                        mapDictionary.ContainsKey(WorldToTile(neighbor)))
                    {
                        Gizmos.DrawLine(tile.position, neighbor);
                    }
                }
                Gizmos.DrawWireCube(tile.position, new Vector3(1f, 0.1f, 1f));
            }
        }
    }
}

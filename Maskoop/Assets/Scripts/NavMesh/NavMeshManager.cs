using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TransformList
{
    public List<Transform> transforms;
}

public enum TileTYpe{
    Ground,
    Air,
    Wall
}
public class TileData
{
    public Transform position;
    public TileTYpe tileType;
}

public class NavMeshManager : MonoBehaviour
{
    [SerializeField]
    public List<TransformList> transformList;

    private List<List<TileData>> tileMatrix;

    private Dictionary<Vector2, TileData> mapDictionary;


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
        foreach(TransformList transList in transformList)
        {
            List<TileData> tiledataAux = new List<TileData>();
            tileMatrix.Add(tiledataAux);
            foreach(Transform tr in transList.transforms)
            {
                TileData data = new TileData();
                data.position = tr;
                data.tileType = TileTYpe.Ground;
                tiledataAux.Add(data);
                mapDictionary.Add(new Vector2(data.position.position.x, data.position.position.z),data);
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector3 GetRandomPointInMap()
    {
        int i = Random.Range(0, tileMatrix.Count);
        int j = Random.Range(0, tileMatrix[i].Count);

        return tileMatrix[i][j].position.position;
    }

    public Vector2 WorldToTile(Vector3 worldPosition)
    {
        Vector3 p = worldPosition;

        return new Vector2(
            Mathf.Floor(p.x) + 0.5f,
            Mathf.Floor(p.z) + 0.5f
        );
    }

    public List<Vector3> FindPath(Vector2 start, Vector2 goal)
    {
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

            foreach (TileData neighbor in GetNeighbors(current))
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

    public bool IsTileWalkable(Vector2 tile)
    {
        return mapDictionary.TryGetValue(tile,out TileData tiledata);
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
    private List<TileData> GetNeighbors(TileData current)
    {
        var neighbors = new List<TileData>();
        foreach (Vector3 dir in directions)
        {
            Vector3 neighborPos = current.position.position + dir;

            if (mapDictionary.TryGetValue(WorldToTile(neighborPos), out TileData neighbor))
            {
                if(neighbor.tileType == TileTYpe.Ground)
                {
                    neighbors.Add(neighbor);
                }
            }
        }
        return neighbors;
    }

    float Heuristic(TileData a, TileData b)
    {
        return Vector3.Distance(a.position.position, b.position.position);
    }


    List<Vector3> ReconstructPath(Dictionary<TileData, TileData> cameFrom, TileData current)
    {
        var path = new List<TileData> { current };
        List<Vector3> positionList = new List<Vector3>();
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
            positionList.Add(current.position.position);
        }
        positionList.Reverse();
        return positionList;
    }

    //Dinamic tyles
    private void SetTileType(Vector3 worldPos, TileTYpe tileType)
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
            bool isBlocked = Physics.CheckBox(tile.position.position + Vector3.up * 0.6f, new Vector3(0.4f, 0.2f, 0.4f));
            TileTYpe newType = isBlocked ? TileTYpe.Wall : TileTYpe.Ground;
            if(newType != tile.tileType)
            {
                SetTileType(tile.position.position, newType);
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
        {
            if (transformList == null) return;

            foreach (TransformList list in transformList)
            {
                if (list.transforms == null) continue;

                foreach (Transform tr in list.transforms)
                {
                    if (tr == null) continue;

                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(tr.position, new Vector3(1f, 0.1f, 1f));
                }
            }
            return;
        }


        foreach (var row in tileMatrix)
        {
            foreach (var tile in row)
            {
                if (tile == null || tile.position == null) continue;

                switch (tile.tileType)
                {
                    case TileTYpe.Ground:
                        Gizmos.color = Color.green;
                        break;

                    case TileTYpe.Wall:
                        Gizmos.color = Color.red;
                        break;

                    case TileTYpe.Air:
                        Gizmos.color = Color.cyan;
                        break;
                }
                foreach (Vector3 dir in directions)
                {
                    Vector3 neighbor = tile.position.position + dir;

                    if (mapDictionary != null &&
                        mapDictionary.ContainsKey(WorldToTile(neighbor)))
                    {
                        Gizmos.DrawLine(tile.position.position, neighbor);
                    }
                }
                Gizmos.DrawWireCube(tile.position.position, new Vector3(1f, 0.1f, 1f));
            }
        }
    }
}

using UnityEngine;

[System.Serializable]
public class TileRow
{
    public TileData[] row;
}

[ExecuteAlways]
public class TileMatrixGenerator : MonoBehaviour
{
    [HideInInspector]
    public TileData[,] tiles;

    [Header("Area Settings")]
    [SerializeField] private int margin = 3;

    [Header("Debug Settings")]
    public float tileSize = 0.75f;          
    public float gizmoHeight = 0.1f;     

    [Header("Debug")]
    [SerializeField] private TileRow[] _tiles;

    private void UpdateDebugTiles()
    {
        if (tiles == null)
        {
            _tiles = null;
            return;
        }

        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        _tiles = new TileRow[height];

        for (int z = 0; z < height; z++)
        {
            _tiles[z] = new TileRow();
            _tiles[z].row = new TileData[width];

            for (int x = 0; x < width; x++)
            {
                _tiles[z].row[x] = tiles[x, z];
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (tiles == null)
            return;

        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                TileData tile = tiles[x, z];
                if (tile == null)
                    continue;

                Vector3 pos = tile.position + Vector3.up * gizmoHeight * 0.5f;

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

                Gizmos.DrawCube(pos, new Vector3(tileSize, gizmoHeight, tileSize));
            }
        }
    }

    [ContextMenu("Generate")]
    public TileData[,] GenerateTiles()
    {
        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        float y = transform.position.y;

        // Create the bounds
        foreach (var t in children)
        {
            if (t.gameObject.layer != LayerMask.NameToLayer("Ground"))
                continue;

            Vector3 p = t.position;

            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minZ = Mathf.Min(minZ, p.z);
            maxZ = Mathf.Max(maxZ, p.z);

            y = Mathf.Max(y, p.y);
        }

        minX -= margin - 1;
        maxX += margin;
        minZ -= margin - 1;
        maxZ += margin;

        int width = Mathf.RoundToInt(maxX - minX);
        int height = Mathf.RoundToInt(maxZ - minZ);

        tiles = new TileData[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(minX + x, y, minZ + z);
                GameObject occupiedBy = null;
                TileType type = TileType.Air;

                Collider[] hits = Physics.OverlapBox(position - Vector3.up * 0.5f, new Vector3(0.25f, 0.5f, 0.25f));

                foreach (var hit in hits)
                {
                    if (hit.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        type = TileType.Ground;
                        occupiedBy = hit.gameObject;
                        break;
                    }
                }

                Collider[] wallHits = Physics.OverlapBox(position + Vector3.up * 1f, new Vector3(0.45f, 0.5f, 0.45f));

                foreach (var hit in wallHits)
                {
                    if (hit.CompareTag("Wall"))
                    {
                        type = TileType.Wall;
                        occupiedBy = hit.gameObject;
                        break;
                    }

                    if (hit.gameObject.GetComponent<SlidingActivable>() !=  null)
                    {
                        type = TileType.Wall;
                        occupiedBy = hit.gameObject;
                        break;
                    }
                }

                tiles[x, z] = new TileData
                {
                    position = position,
                    tileType = type,
                    occupiedBy = occupiedBy
                };
            }
        }

        UpdateDebugTiles();

        return tiles;
    }

    [ContextMenu("Clear")]
    private void ClearTiles()
    {
        tiles = null;
        UpdateDebugTiles();
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates, updates, and removes scene instances for the blocks stored in a <see cref="MapDataSO"/> asset.
/// </summary>
public static class MapRenderer
{
    /// <summary>
    /// Name used for the generated root GameObject that holds all rendered map blocks.
    /// </summary>
    private const string k_rootName = "MapRenderer_Root";

    /// <summary>
    /// Tag used to identify wall blocks for axis-based grouping. Must exist in the project's Tag Manager.
    /// </summary>
    private const string k_wallTag = "Wall";

    /// <summary>
    /// Tag used to identify ground blocks for area grouping under each layer. Must exist in the project's Tag Manager.
    /// </summary>
    private const string k_groundTag = "Ground";


    /// <summary>
    /// Rebuilds every rendered block instance in the scene from the supplied map data.
    /// </summary>
    public static void RebuildAll(MapDataSO map)
    {
        if (map == null)
        {
            DestroyRoot();
            return;
        }

        DestroyRoot();
        var root = GetOrCreateRoot();

        var blockLookup = BuildBlockLookup();

        // Sort cells into the three categories that drive the scene hierarchy.
        var groundByLayer = new Dictionary<int, HashSet<Vector3Int>>();
        var wallCells = new HashSet<Vector3Int>();
        var otherByLayer = new Dictionary<int, List<Vector3Int>>();

        for (int z = 0; z < map.Depth; z++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var cell = map.GetCell(x, y, z);
                    if (cell.IsEmpty || string.IsNullOrEmpty(cell.BlockId)) continue;
                    if (!blockLookup.TryGetValue(cell.BlockId, out var def) || def.Prefab == null) continue;

                    var coord = new Vector3Int(x, y, z);
                    string tag = def.Prefab.tag;

                    if (tag == k_wallTag)
                    {
                        wallCells.Add(coord);
                    }
                    else if (tag == k_groundTag)
                    {
                        if (!groundByLayer.TryGetValue(y, out var set))
                        {
                            set = new HashSet<Vector3Int>();
                            groundByLayer[y] = set;
                        }
                        set.Add(coord);
                    }
                    else
                    {
                        if (!otherByLayer.TryGetValue(y, out var list))
                        {
                            list = new List<Vector3Int>();
                            otherByLayer[y] = list;
                        }
                        list.Add(coord);
                    }
                }
            }
        }

        BuildLayerHierarchy(map, blockLookup, root, groundByLayer, otherByLayer);
        BuildWallHierarchy(map, blockLookup, root, wallCells);
    }


    /// <summary>
    /// Saves the currently rendered map blocks as a new prefab asset.
    /// </summary>
    public static void SaveAsPrefab()
    {
        var root = GameObject.Find(k_rootName);
        if (root == null)
        {
            EditorUtility.DisplayDialog("Save Prefab", "No map is currently rendered in the scene.", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject("Save Map Prefab", "NewMap", "prefab", "Choose where to save the map prefab");
        if (string.IsNullOrEmpty(path)) return;

        PrefabUtility.SaveAsPrefabAsset(root, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Map saved as prefab at {path}");
    }


    /// <summary>
    /// Refreshes the rendered block instance for a single cell.
    /// </summary>
    public static void RefreshCell(MapDataSO map, Vector3Int cell)
    {
        // A full rebuild keeps the hierarchy correct.
        RebuildAll(map);
    }


    /// <summary>
    /// Instantiates a prefab for the given block definition and parents it under the map root.
    /// </summary>
    private static void SpawnBlock(BlockDefinitionSO def, Vector3Int cell, GameObject root)
    {
        if (def.Prefab == null) return;

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(def.Prefab);
        instance.name = CellName(cell);
        instance.transform.SetParent(root.transform);
        instance.transform.position = cell;
    }


    /// <summary>
    /// Gets the existing map root GameObject, or creates it if none exists.
    /// </summary>
    /// <returns>The map render root GameObject.</returns>
    private static GameObject GetOrCreateRoot()
    {
        var existing = GameObject.Find(k_rootName);
        if (existing != null) return existing;

        var root = new GameObject(k_rootName);
        return root;
    }


    /// <summary>
    /// Destroys the current map root GameObject if it exists.
    /// </summary>
    private static void DestroyRoot()
    {
        var root = GameObject.Find(k_rootName);
        if (root != null)
            Object.DestroyImmediate(root);
    }


    /// <summary>
    /// Builds a unique scene object name for a cell coordinate.
    /// </summary>
    /// <returns>A stable name in the format <c>block_x_y_z</c>.</returns>
    private static string CellName(Vector3Int cell) => $"block_{cell.x}_{cell.y}_{cell.z}";


    /// <summary>
    /// Builds the per-layer hierarchy with grouped floor areas and any uncategorized blocks.
    /// </summary>
    private static void BuildLayerHierarchy(
        MapDataSO map,
        Dictionary<string, BlockDefinitionSO> lookup,
        GameObject root,
        Dictionary<int, HashSet<Vector3Int>> groundByLayer,
        Dictionary<int, List<Vector3Int>> otherByLayer)
    {
        // Union of every Y that has at least one ground or other block.
        var allLayers = new HashSet<int>();
        foreach (var k in groundByLayer.Keys) allLayers.Add(k);
        foreach (var k in otherByLayer.Keys) allLayers.Add(k);

        foreach (int layer in allLayers)
        {
            var layerParent = new GameObject($"Layer_{layer}");
            layerParent.transform.SetParent(root.transform);

            // Group adjacent ground tiles into "Area_N" containers.
            if (groundByLayer.TryGetValue(layer, out var floorCells))
            {
                var areas = FindFloorAreas(floorCells);
                for (int i = 0; i < areas.Count; i++)
                {
                    var areaParent = new GameObject($"Floor_{i}");
                    areaParent.transform.SetParent(layerParent.transform);
                    areaParent.AddComponent<TileMatrixGenerator>();
                    SpawnCells(map, lookup, areas[i], areaParent);
                }
            }

            // Untagged blocks (not Wall, not Ground) sit directly under the layer parent.
            if (otherByLayer.TryGetValue(layer, out var others))
                SpawnCells(map, lookup, others, layerParent);
        }
    }


    /// <summary>
    /// Builds the "Walls" hierarchy with each axis-aligned wall run as its own group.
    /// </summary>
    private static void BuildWallHierarchy(
        MapDataSO map,
        Dictionary<string, BlockDefinitionSO> lookup,
        GameObject root,
        HashSet<Vector3Int> wallCells)
    {
        if (wallCells.Count == 0) return;

        var wallsParent = new GameObject("Walls");
        wallsParent.transform.SetParent(root.transform);

        var groups = GroupWallsByAxis(wallCells);
        for (int i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            string axis = DetermineAxis(group);
            var wallParent = new GameObject($"Wall_{axis}_{i}");
            wallParent.transform.SetParent(wallsParent.transform);
            SpawnCells(map, lookup, group, wallParent);
        }
    }


    /// <summary>
    /// Builds a lookup from block ID to its <see cref="BlockDefinitionSO"/> asset.
    /// </summary>
    private static Dictionary<string, BlockDefinitionSO> BuildBlockLookup()
    {
        var lookup = new Dictionary<string, BlockDefinitionSO>();
        string[] guids = AssetDatabase.FindAssets("t:BlockDefinitionSO");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var block = AssetDatabase.LoadAssetAtPath<BlockDefinitionSO>(path);
            if (block != null && !string.IsNullOrEmpty(block.BlockId))
                lookup[block.BlockId] = block;
        }

        return lookup;
    }


    /// <summary>
    /// Spawns prefab instances for each cell coordinate under the supplied parent.
    /// </summary>
    private static void SpawnCells(MapDataSO map, Dictionary<string, BlockDefinitionSO> lookup, IEnumerable<Vector3Int> cells, GameObject parent)
    {
        foreach (var cell in cells)
        {
            var data = map.GetCell(cell.x, cell.y, cell.z);
            if (lookup.TryGetValue(data.BlockId, out var def))
                SpawnBlock(def, cell, parent);
        }
    }


    /// <summary>
    /// Finds connected components of ground tiles within a single Y layer using 4-way adjacency.
    /// </summary>
    private static List<List<Vector3Int>> FindFloorAreas(HashSet<Vector3Int> floorCells)
    {
        var areas = new List<List<Vector3Int>>();
        var visited = new HashSet<Vector3Int>();

        foreach (var cell in floorCells)
        {
            if (visited.Contains(cell)) continue;

            var area = new List<Vector3Int>();
            var queue = new Queue<Vector3Int>();
            queue.Enqueue(cell);
            visited.Add(cell);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                area.Add(current);

                // Same height 4-way neighbors keep each area confined to a single layer.
                TryEnqueue(queue, visited, floorCells, current + new Vector3Int(1, 0, 0));
                TryEnqueue(queue, visited, floorCells, current + new Vector3Int(-1, 0, 0));
                TryEnqueue(queue, visited, floorCells, current + new Vector3Int(0, 0, 1));
                TryEnqueue(queue, visited, floorCells, current + new Vector3Int(0, 0, -1));
            }

            areas.Add(area);
        }

        return areas;
    }


    /// <summary>
    /// Adds a candidate cell to the BFS queue if it belongs to the source set and has not been visited.
    /// </summary>
    private static void TryEnqueue(Queue<Vector3Int> queue, HashSet<Vector3Int> visited, HashSet<Vector3Int> source, Vector3Int cell)
    {
        if (!source.Contains(cell) || visited.Contains(cell)) return;
        visited.Add(cell);
        queue.Enqueue(cell);
    }


    /// <summary>
    /// Splits wall cells into maximal runs along a single axis (X first, then Z).
    /// </summary>
    private static List<List<Vector3Int>> GroupWallsByAxis(HashSet<Vector3Int> wallCells)
    {
        var horizontalRuns = new List<List<Vector3Int>>();
        var visited = new HashSet<Vector3Int>();

        // X-aligned horizontal runs.
        foreach (var cell in wallCells)
        {
            if (visited.Contains(cell)) continue;

            bool hasXNeighbor = wallCells.Contains(cell + new Vector3Int(1, 0, 0))
                              || wallCells.Contains(cell + new Vector3Int(-1, 0, 0));

            if (!hasXNeighbor) continue;

            horizontalRuns.Add(ExtendRun(cell, new Vector3Int(1, 0, 0), wallCells, visited));
        }

        // Z-aligned horizontal runs and singleton cells.
        foreach (var cell in wallCells)
        {
            if (visited.Contains(cell)) continue;
            horizontalRuns.Add(ExtendRun(cell, new Vector3Int(0, 0, 1), wallCells, visited));
        }

        // Bucket runs by horizontal footprint, then merge vertically adjacent ones.
        var byFootprint = new Dictionary<string, List<List<Vector3Int>>>();
        foreach (var run in horizontalRuns)
        {
            string key = MakeFootprintKey(run);
            if (!byFootprint.TryGetValue(key, out var bucket))
            {
                bucket = new List<List<Vector3Int>>();
                byFootprint[key] = bucket;
            }
            bucket.Add(run);
        }

        var finalGroups = new List<List<Vector3Int>>();
        foreach (var bucket in byFootprint.Values)
        {
            // Sort by Y so we can fold contiguous layers into a single group.
            bucket.Sort((a, b) => a[0].y.CompareTo(b[0].y));

            var current = new List<Vector3Int>(bucket[0]);
            int prevY = bucket[0][0].y;

            for (int i = 1; i < bucket.Count; i++)
            {
                int y = bucket[i][0].y;
                if (y == prevY + 1)
                {
                    current.AddRange(bucket[i]);
                }
                else
                {
                    finalGroups.Add(current);
                    current = new List<Vector3Int>(bucket[i]);
                }
                prevY = y;
            }
            finalGroups.Add(current);
        }

        return finalGroups;
    }


    /// <summary>
    /// Greedy run extension along a single axis from a starting cell, in both directions.
    /// </summary>
    private static List<Vector3Int> ExtendRun(Vector3Int start, Vector3Int axis, HashSet<Vector3Int> cells, HashSet<Vector3Int> visited)
    {
        var run = new List<Vector3Int> { start };
        visited.Add(start);

        // Forward direction appended at the tail.
        Vector3Int next = start + axis;
        while (cells.Contains(next) && !visited.Contains(next))
        {
            run.Add(next);
            visited.Add(next);
            next += axis;
        }

        // Backward direction prepended at the head.
        next = start - axis;
        while (cells.Contains(next) && !visited.Contains(next))
        {
            run.Insert(0, next);
            visited.Add(next);
            next -= axis;
        }

        return run;
    }


    /// <summary>
    /// Builds a key that identifies a horizontal run by axis, fixed coord, and range,
    /// so runs on different Y levels can be matched and merged vertically.
    /// </summary>
    private static string MakeFootprintKey(List<Vector3Int> run)
    {
        var first = run[0];
        var last = run[run.Count - 1];

        if (first.x != last.x)
        {
            int xMin = Mathf.Min(first.x, last.x);
            int xMax = Mathf.Max(first.x, last.x);
            return $"X|{first.z}|{xMin}-{xMax}";
        }

        if (first.z != last.z)
        {
            int zMin = Mathf.Min(first.z, last.z);
            int zMax = Mathf.Max(first.z, last.z);
            return $"Z|{first.x}|{zMin}-{zMax}";
        }

        // Singleton block keyed by (x, z) so vertical pillars merge.
        return $"P|{first.x}|{first.z}";
    }


    /// <summary>
    /// Returns "X", "Z", or "Y" depending on the dominant axis of a wall group.
    /// </summary>
    private static string DetermineAxis(List<Vector3Int> group)
    {
        int minX = int.MaxValue, maxX = int.MinValue;
        int minZ = int.MaxValue, maxZ = int.MinValue;

        foreach (var c in group)
        {
            if (c.x < minX) minX = c.x;
            if (c.x > maxX) maxX = c.x;
            if (c.z < minZ) minZ = c.z;
            if (c.z > maxZ) maxZ = c.z;
        }

        int dx = maxX - minX;
        int dz = maxZ - minZ;

        if (dx == 0 && dz == 0) return "Y";
        return dx >= dz ? "X" : "Z";
    }
}
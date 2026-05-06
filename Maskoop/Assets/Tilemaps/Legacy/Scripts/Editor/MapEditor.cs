using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
public class MapEditor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Grid mapGrid;
    [SerializeField] GameObject mapOrigin;

    [Header("Save Settings")]
    [SerializeField] string mapName = "ExampleMap";
    [SerializeField] string mapFolder = "Assets/Prefabs/Maps";
    [SerializeField] string gridFolder = "Assets/Tilemaps/Maps";

    [Header("Brush Prefabs")]
    [SerializeField] GameObject floorBrush;
    [SerializeField] GameObject wallBrush;
    [SerializeField] GameObject bridgeBrush;
    [SerializeField] GameObject doorBrush;

    [Header("Final Prefabs")]
    [SerializeField] GameObject floorPrefab;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject bridgePrefab;
    [SerializeField] GameObject doorPrefab;

    enum WallOrientation
    {
        Horizontal,
        Vertical
    }

    void ValidateFolder(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }
    }

    bool ValidateRoot()
    {
        if (mapOrigin != null)
        {
            // Debug.Log("Map prefab is already assigned.");
            return false;
        }

        // Debug.Log("Map prefab is not assigned.");
        return true;
    }

    GameObject GenerateRoot()
    {
        GameObject root = new GameObject(mapName);
        root.transform.position = mapGrid.transform.position;
        root.AddComponent<WallEditor>();
        return root;
    }

    void GenerateLevel(Tilemap level, int index, Transform root)
    {
        Transform[] levelChildren = level.GetComponentsInChildren<Transform>(true);

        GameObject levelParent = new GameObject($"Level_{index:D2}");
        levelParent.transform.SetParent(root, false);

        GameObject floorParent = new GameObject("Floor");
        floorParent.transform.SetParent(levelParent.transform, false);

        GameObject wallParent = new GameObject("Wall");
        wallParent.transform.SetParent(levelParent.transform, false);

        GameObject bridgeParent = new GameObject("Bridge");
        bridgeParent.transform.SetParent(levelParent.transform, false);

        GameObject doorParent = new GameObject("Door");
        doorParent.transform.SetParent(levelParent.transform, false);

        GenerateFloors(levelChildren, floorParent.transform);
        GenerateWalls(level, levelChildren, wallParent.transform);
        GenerateBridges(levelChildren, bridgeParent.transform);
        GenerateDoors(levelChildren, doorParent.transform);
    }

    void GenerateFloors(Transform[] levelChildren, Transform floorRoot)
    {
        List<Transform> floorTiles = new List<Transform>();

        foreach (Transform child in levelChildren)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);

            if (source == floorBrush)
            {
                floorTiles.Add(child);
            }

            if (source == doorBrush)
            {
                floorTiles.Add(child);
            }
        }

        HashSet<Transform> visited = new HashSet<Transform>();

        int areaIndex = 1;

        foreach (Transform tile in floorTiles)
        {
            if (visited.Contains(tile))
                continue;

            List<Transform> areaTiles = CollectConnectedFloorBlocks(tile, floorTiles, visited);

            GameObject areaObject = new GameObject($"Area_{areaIndex:00}");
            areaObject.AddComponent<TileMatrixGenerator>();
            areaObject.transform.SetParent(floorRoot);

            foreach (Transform areaTile in areaTiles)
            {
                GameObject floor = PrefabUtility.InstantiatePrefab(floorPrefab) as GameObject;

                floor.transform.position = areaTile.position;
                floor.transform.rotation = areaTile.rotation;
                floor.transform.SetParent(areaObject.transform);
            }

            areaIndex++;
        }
    }

    void GenerateWalls(Tilemap level, Transform[] levelChildren, Transform wallParent)
    {
        List<Transform> wallBlocks = CollectWallBlocks(level, levelChildren);

        HashSet<Transform> processed = new HashSet<Transform>();

        ProcessWallAxis(
            wallBlocks,
            processed,
            t => new Vector2(t.position.z, t.position.y),
            t => t.position.x,
            (a, b) => Mathf.Abs(a.position.x - b.position.x),
            WallOrientation.Vertical,
            wallParent
        );

        ProcessWallAxis(
            wallBlocks.Where(t => !processed.Contains(t)).ToList(),
            processed,
            t => new Vector2(t.position.x, t.position.y),
            t => t.position.z,
            (a, b) => Mathf.Abs(a.position.z - b.position.z),
            WallOrientation.Horizontal,
            wallParent
        );

        foreach (Transform t in wallBlocks)
        {
            if (processed.Contains(t))
                continue;

            Instantiate(wallPrefab, t.position, t.rotation, wallParent);
        }
    }

    void GenerateBridges(Transform[] levelChildren, Transform bridgesRootParent)
    {
        List<Transform> bridgeTiles = new List<Transform>();

        foreach (Transform child in levelChildren)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);

            if (source == bridgeBrush)
                bridgeTiles.Add(child);
        }

        HashSet<Transform> visited = new HashSet<Transform>();

        foreach (Transform tile in bridgeTiles)
        {
            if (visited.Contains(tile))
                continue;

            List<Transform> segment = CollectBridgeBlocks(tile, bridgeTiles, visited);

            ProcessBridgeSegment(segment, bridgesRootParent);
        }
    }

    void GenerateDoors(Transform[] levelChildren, Transform doorRootParent)
    {
        List<Transform> doorTiles = new List<Transform>();

        foreach (Transform child in levelChildren)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);

            if (source == doorBrush)
                doorTiles.Add(child);
        }

        HashSet<Transform> visited = new HashSet<Transform>();

        foreach (Transform tile in doorTiles)
        {
            if (visited.Contains(tile))
                continue;

            List<Transform> segment = CollectDoorBlocks(tile, doorTiles, visited);

            ProcessDoorSegment(segment, doorRootParent);
        }
    }

    [ContextMenu("Generate map")]
    void GenerateMap()
    {
        if (!ValidateRoot())
            return;

        Tilemap[] levels = mapGrid.GetComponentsInChildren<Tilemap>();

        GameObject root = GenerateRoot();

        for (int i = 0; i < levels.Length; i++)
        {
            GenerateLevel(levels[i], i, root.transform);
        }

        mapGrid.gameObject.SetActive(false);
        mapOrigin = root;
        mapOrigin.GetComponent<WallEditor>().Adjust();
    }

    [ContextMenu("Save map")]
    void SaveMap()
    {
        if (ValidateRoot())
            return;

        ValidateFolder(mapFolder);

        string mapPath = GetUniquePath(mapFolder, mapName);
        PrefabUtility.SaveAsPrefabAsset(mapOrigin, mapPath);
        Debug.Log($"Map saved: {mapPath}");
    }

    [ContextMenu("Clear map")]
    void ClearMap()
    {
        if (ValidateRoot())
            return;

        DestroyImmediate(mapOrigin);

        mapOrigin = null;

        mapGrid.gameObject.SetActive(true);
    }

    [ContextMenu("Save grid")]
    void SaveGrid()
    {
        ValidateFolder(gridFolder);

        string gridPath = GetUniquePath(gridFolder, mapName);
        PrefabUtility.SaveAsPrefabAsset(mapGrid.gameObject, gridPath);
        Debug.Log($"Grid saved: {gridPath}");
    }

    [ContextMenu("Clear grid")]
    void ClearGrid()
    {
        Tilemap[] levels = mapGrid.GetComponentsInChildren<Tilemap>();

        foreach (Tilemap level in levels)
        {
            Transform t = level.transform;

            for (int i = t.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(t.GetChild(i).gameObject);
            }
        }

        mapGrid.gameObject.SetActive(true);
    }

    string GetUniquePath(string folder, string baseName)
    {
        string path = $"{folder}/{baseName}.prefab";
        int index = 1;

        while (File.Exists(path))
        {
            path = $"{folder}/{baseName}_{index}.prefab";
            index++;
        }

        return path;
    }

    List<Transform> CollectConnectedFloorBlocks(
        Transform start,
        List<Transform> allTiles,
        HashSet<Transform> visited)
    {
        List<Transform> result = new List<Transform>();
        Queue<Transform> queue = new Queue<Transform>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            result.Add(current);

            foreach (Transform other in allTiles)
            {
                if (visited.Contains(other))
                    continue;

                if (Vector3.Distance(current.position, other.position) < 1.1f)
                {
                    visited.Add(other);
                    queue.Enqueue(other);
                }
            }
        }

        return result;
    }

    List<Transform> CollectBridgeBlocks(
        Transform start,
        List<Transform> allTiles,
        HashSet<Transform> visited)
    {
        List<Transform> segment = new List<Transform>();
        Queue<Transform> queue = new Queue<Transform>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            segment.Add(current);

            foreach (Transform other in allTiles)
            {
                if (visited.Contains(other))
                    continue;

                if (Vector3.Distance(current.position, other.position) < 1.1f)
                {
                    visited.Add(other);
                    queue.Enqueue(other);
                }
            }
        }

        return segment;
    }

    void ProcessBridgeSegment(List<Transform> segment, Transform bridgeParent)
    {
        if (segment.Count <= 1)
            return;

        Vector3 min = segment[0].position;
        Vector3 max = segment[0].position;

        foreach (Transform t in segment)
        {
            min = Vector3.Min(min, t.position);
            max = Vector3.Max(max, t.position);
        }

        Vector3 size = max - min;

        bool alongX = size.x > size.z;
        float length = alongX ? size.x : size.z;
        float width = alongX ? size.z : size.x; ;

        Vector3 pivot;

        if (alongX)
        {
            pivot = new Vector3(min.x - 0.5f, min.y - 0.501f, (min.z + max.z) * 0.5f);
        }
        else
        {
            pivot = new Vector3((min.x + max.x) * 0.5f, min.y - 0.501f, min.z - 0.5f);
        }

        Quaternion rotation = alongX
            ? Quaternion.Euler(0, 90, 0)
            : Quaternion.identity;

        GameObject bridge = PrefabUtility.InstantiatePrefab(bridgePrefab) as GameObject;

        bridge.transform.SetParent(bridgeParent);
        bridge.transform.position = pivot;
        bridge.transform.rotation = rotation;

        Vector3 scale = bridge.transform.localScale;

        scale.z = length + 1;
        scale.x = width + 1;

        bridge.transform.localScale = scale;
    }

    List<Transform> CollectDoorBlocks(
    Transform start,
    List<Transform> allTiles,
    HashSet<Transform> visited)
    {
        List<Transform> segment = new List<Transform>();
        Queue<Transform> queue = new Queue<Transform>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            segment.Add(current);

            foreach (Transform other in allTiles)
            {
                if (visited.Contains(other))
                    continue;

                if (Vector3.Distance(current.position, other.position) < 1.1f)
                {
                    visited.Add(other);
                    queue.Enqueue(other);
                }
            }
        }

        return segment;
    }

    void ProcessDoorSegment(List<Transform> segment, Transform doorParent)
    {
        if (segment.Count <= 1)
            return;

        Vector3 min = segment[0].position;
        Vector3 max = segment[0].position;

        foreach (Transform t in segment)
        {
            min = Vector3.Min(min, t.position);
            max = Vector3.Max(max, t.position);
        }

        Vector3 size = max - min;

        bool alongX = size.x > size.z;
        float length = alongX ? size.x : size.z;
        float width = alongX ? size.z : size.x; ;

        Vector3 pivot = (min + max) * 0.5f;

        Quaternion rotation = alongX
            ? Quaternion.Euler(0, 90, 0)
            : Quaternion.identity;

        GameObject door = PrefabUtility.InstantiatePrefab(doorPrefab) as GameObject;

        door.transform.SetParent(doorParent);
        door.transform.position = pivot;
        door.transform.rotation = rotation;

        Vector3 scale = door.transform.localScale;

        scale.z = length + 1;
        scale.x = width + 1;

        door.transform.localScale = scale;
    }

    List<Transform> CollectWallBlocks(Tilemap level, Transform[] children)
    {
        List<Transform> wallBlocks = new List<Transform>();

        foreach (Transform child in children)
        {
            if (child == level.transform)
                continue;

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);

            if (source == wallBrush)
                wallBlocks.Add(child);
        }

        return wallBlocks;
    }

    void ProcessWallAxis(
        List<Transform> blocks,
        HashSet<Transform> processed,
        Func<Transform, Vector2> groupKey,
        Func<Transform, float> orderKey,
        Func<Transform, Transform, float> distance,
        WallOrientation orientation,
        Transform wallParent)
    {
        var groups = blocks.GroupBy(groupKey);

        foreach (var group in groups)
        {
            var ordered = group.OrderBy(orderKey).ToList();

            List<Transform> segment = new List<Transform>();

            for (int i = 0; i < ordered.Count; i++)
            {
                if (segment.Count == 0)
                {
                    segment.Add(ordered[i]);
                    continue;
                }

                float d = distance(ordered[i], segment.Last());

                if (Mathf.Approximately(d, 1f))
                    segment.Add(ordered[i]);
                else
                {
                    ProcessWallSegment(segment, orientation, processed, wallParent);
                    segment.Clear();
                    segment.Add(ordered[i]);
                }
            }

            ProcessWallSegment(segment, orientation, processed, wallParent);
        }
    }

    void ProcessWallSegment(
        List<Transform> segment,
        WallOrientation orientation,
        HashSet<Transform> processed,
        Transform wallParent)
    {
        if (segment.Count <= 1)
            return;

        Vector3 start = segment.First().position;
        Vector3 end = segment.Last().position;

        Vector3 center = (start + end) / 2f;

        GameObject wall = Instantiate(wallPrefab, center, Quaternion.identity, wallParent);

        Vector3 scale = wall.transform.localScale;

        if (orientation == WallOrientation.Vertical)
            scale.x = segment.Count;
        else if (orientation == WallOrientation.Horizontal)
            scale.z = segment.Count;
        else
        {
            Debug.LogError("Invalid wall orientation");
            return;
        }

        wall.transform.localScale = scale;

        foreach (var t in segment)
            processed.Add(t);
    }
}

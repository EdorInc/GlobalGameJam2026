using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using static Unity.VisualScripting.Metadata;

public class MapEditor : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] Grid mapGrid;

    [Header("Save Settings")]
    [SerializeField] string mapName = "ExampleMap";
    [SerializeField] string mapFolder = "Assets/Prefabs/Maps";
    [SerializeField] string gridFolder = "Assets/Tilemaps/Maps";

    [Header("Brush Prefabs")]
    [SerializeField] GameObject floorBrush;
    [SerializeField] GameObject wallBrush;

    [Header("Final Prefabs")]
    [SerializeField] GameObject floorPrefab;
    [SerializeField] GameObject wallPrefab;

    private GameObject mapPrefab;

    enum WallOrientation
    {
        Horizontal,
        Vertical
    }

    void EnsureFolder(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }
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

    [ContextMenu("Save grid")]
    void SaveGrid()
    {
        EnsureFolder(gridFolder);

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
    }

    [ContextMenu("Generate map")]
    void GenerateMap()
    {
        if (mapPrefab != null)
        {
            Debug.LogError("Map prefab is already assigned.");
            return;
        }

        Tilemap[] levels = mapGrid.GetComponentsInChildren<Tilemap>();

        // Root that will contain all levels
        GameObject root = new GameObject(mapName);
        root.transform.position = mapGrid.transform.position;
        WallEditor wallEditor = root.AddComponent<WallEditor>();

        for (int i = 0; i < levels.Length; i++)
        {
            Tilemap level = levels[i];
            Transform[] levelChildren = level.GetComponentsInChildren<Transform>(true);

            // Create level root
            GameObject levelParent = new GameObject($"Level_{i:D2}");
            levelParent.transform.SetParent(root.transform, false);

            // Create children
            GameObject floorParent = new GameObject("Floor");
            floorParent.transform.SetParent(levelParent.transform, false);
            GameObject wallParent = new GameObject("Wall");
            wallParent.transform.SetParent(levelParent.transform, false);

            // Get all the transforms of the floor blocks in the tilemap
            foreach (Transform floorChild in levelChildren)
            {
                GameObject floorSource = PrefabUtility.GetCorrespondingObjectFromSource(floorChild.gameObject);

                if (floorSource == floorBrush)
                {
                    // Add a floor prefab with the same position and rotation as the floor block
                    Instantiate(
                        floorPrefab,
                        floorChild.position,
                        floorChild.rotation,
                        floorParent.transform // Make the floor block children of a empty called "Floor"
                    );
                }
            }

            // Get all wall blocks in the tilemap
            List<Transform> wallBlocks = new List<Transform>();

            HashSet<Transform> processed = new HashSet<Transform>();

            // Get all the transforms of the wall blocks in the tilemap
            foreach (Transform wallChild in levelChildren)
                {
                    if (wallChild == level.transform)
                        continue;

                    GameObject wallSource = PrefabUtility.GetCorrespondingObjectFromSource(wallChild.gameObject);

                    if (wallSource == wallBrush)
                        wallBlocks.Add(wallChild);
                }

            // Gather all transforms lined on the X axis with more than one block in between them
            var xGroups = wallBlocks.GroupBy(t => new Vector2(t.position.z, t.position.y));

            foreach (var group in xGroups)
                {
                    var ordered = group.OrderBy(t => t.position.x).ToList();

                    List<Transform> segment = new List<Transform>();

                    for (int j = 0; j < ordered.Count; j++)
                    {
                        if (segment.Count == 0)
                            segment.Add(ordered[j]);
                        else
                        {
                            float dx = Mathf.Abs(ordered[j].position.x - segment.Last().position.x);

                            if (Mathf.Approximately(dx, 1f))
                                segment.Add(ordered[j]);
                            else
                            {
                                ProcessWallSegment(segment, WallOrientation.Vertical);
                                segment.Clear();
                                segment.Add(ordered[j]);
                            }
                        }
                    }

                    ProcessWallSegment(segment, WallOrientation.Vertical);
                }

            // Gather the transforms lined on the Z axis with more than one block in between them
            var remaining = wallBlocks.Where(t => !processed.Contains(t)).ToList();

            var zGroups = remaining.GroupBy(t => new Vector2(t.position.x, t.position.y));

            foreach (var group in zGroups)
                {
                    var ordered = group.OrderBy(t => t.position.z).ToList();

                    List<Transform> segment = new List<Transform>();

                    for (int k = 0; k < ordered.Count; k++)
                    {
                        if (segment.Count == 0)
                            segment.Add(ordered[k]);
                        else
                        {
                            float dz = Mathf.Abs(ordered[k].position.z - segment.Last().position.z);

                            if (Mathf.Approximately(dz, 1f))
                                segment.Add(ordered[k]);
                            else
                            {
                                ProcessWallSegment(segment, WallOrientation.Horizontal);
                                segment.Clear();
                                segment.Add(ordered[k]);
                            }
                        }
                    }

                    ProcessWallSegment(segment, WallOrientation.Horizontal);
                }

            void ProcessWallSegment(List<Transform> segment, WallOrientation orientation)
            {
                if (segment.Count <= 1)
                    return;

                Vector3 start = segment.First().position;
                Vector3 end = segment.Last().position;

                Vector3 center = (start + end) / 2f;

                // Add a wall prefab within the center of the gathered transforms and modify the scale of the wall prefab
                GameObject wall = Instantiate(wallPrefab, center, Quaternion.identity, wallParent.transform);

                Vector3 scale = wall.transform.localScale;
                
                if (orientation == WallOrientation.Vertical) 
                { 
                    scale.x = segment.Count; 
                } 
                else if (orientation == WallOrientation.Horizontal) 
                { 
                    scale.z = segment.Count; 
                }
                else 
                { 
                    Debug.LogError("Invalid wall orientation");
                    return;
                }
                    

                wall.transform.localScale = scale;

                // Delete the gathered transforms
                foreach (var t in segment)
                {
                    processed.Add(t);
                }
            }

            // Add wall prefabs on the remaining transforms
            foreach (Transform t in wallBlocks)
                {
                    if (processed.Contains(t))
                        continue;

                    // Make the wall blocks children of a empty called "Wall"
                    Instantiate(wallPrefab, t.position, t.rotation, wallParent.transform);
                }
            }

        mapGrid.gameObject.SetActive(false);
        mapPrefab = root;
    }

    [ContextMenu("Save map")]
    void SaveMap()
    {
        if (mapPrefab == null)
        {
            Debug.LogError("Map prefab is not assigned.");
            return;
        }

        EnsureFolder(mapFolder);

        string mapPath = GetUniquePath(mapFolder, mapName);
        PrefabUtility.SaveAsPrefabAsset(mapPrefab, mapPath);
        Debug.Log($"Map saved: {mapPath}");
    }

    [ContextMenu("Clear map")]
    void ClearMap()
    {
        if (mapPrefab == null)
        {
            Debug.LogError("Map prefab is not assigned.");
            return;
        }

        DestroyImmediate(mapPrefab);

        mapPrefab = null;
    }
}

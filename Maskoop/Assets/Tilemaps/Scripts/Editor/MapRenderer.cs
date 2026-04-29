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
    /// Rebuilds every rendered block instance in the scene from the supplied map data.
    /// </summary>
    public static void RebuildAll(MapDataSO map)
    {
        if (map == null) return;

        DestroyRoot();
        var root = GetOrCreateRoot();

        // Build a lookup so map cell block IDs can be resolved to their block definitions.
        string[] guids = AssetDatabase.FindAssets("t:BlockDefinitionSO");
        var blockLookup = new Dictionary<string, BlockDefinitionSO>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var block = AssetDatabase.LoadAssetAtPath<BlockDefinitionSO>(path);
            blockLookup[block.BlockId] = block;
        }

        // Walk the full 3D grid and spawn an instance for every non-empty cell.
        for (int z = 0; z < map.Depth; z++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var cell = map.GetCell(x, y, z);

                    if (cell.IsEmpty || string.IsNullOrEmpty(cell.BlockId)) continue;

                    if (blockLookup.TryGetValue(cell.BlockId, out var def))
                        SpawnBlock(def, new Vector3Int(x, y, z), root);
                }
            }
        }
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
        var root = GetOrCreateRoot();

        // Remove any existing instance at this cell before respawning updated content.
        var existing = root.transform.Find(CellName(cell));
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        var data = map.GetCell(cell.x, cell.y, cell.z);
        if (data.IsEmpty) return;

        // Resolve the cell's block ID back to a block definition and spawn its prefab.
        string[] guids = AssetDatabase.FindAssets($"t:BlockDefinitionSO");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var def = AssetDatabase.LoadAssetAtPath<BlockDefinitionSO>(path);
            if (def.BlockId == data.BlockId)
            {
                SpawnBlock(def, cell, root);
                break;
            }
        }
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
}
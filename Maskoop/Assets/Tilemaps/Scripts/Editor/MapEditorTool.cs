using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Scene view editing utility that places and erases blocks in a <see cref="MapDataSO"/> asset.
/// </summary>
[InitializeOnLoad]
public static class MapEditorTool
{
    /// <summary>
    /// The map currently being edited.
    /// </summary>
    private static MapDataSO s_currentMap;

    /// <summary>
    /// The block currently selected for placement.
    /// </summary>
    private static BlockDefinitionSO s_selectedBlock;

    /// <summary>
    /// Temporary preview instance shown in the Scene view before placement.
    /// </summary>
    private static GameObject s_ghostInstance;

    /// <summary>
    /// The active Y layer used when placing blocks on the grid plane.
    /// </summary>
    private static int s_currentLayer = 0;

    /// <summary>
    /// True when the tool is in erase mode instead of place mode.
    /// </summary>
    private static bool s_isErasing = false;

    /// <summary>
    /// Toggles whether the grid lines are drawn in the Scene view.
    /// </summary>
    private static bool s_showGrid = true;

    /// <summary>
    /// Registers the scene GUI callback when the editor loads.
    /// </summary>
    static MapEditorTool()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    public static void SetBlock(BlockDefinitionSO block) => s_selectedBlock = block;

    public static void SetLayer(int layer) => s_currentLayer = layer;

    public static void SetErasing(bool erasing) => s_isErasing = erasing;

    public static void SetShowGrid(bool show) => s_showGrid = show;


    /// <summary>
    /// Map key for storing the currently edited map's asset GUID in the session state.
    /// </summary>
    private const string k_mapGuidKey = "MapEditor_MapGuid";

    public static void SetMap(MapDataSO map)
    {
        s_currentMap = map;

        // Persist the asset GUID so we can restore it after focus loss
        string guid = map != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(map)) : "";
        SessionState.SetString(k_mapGuidKey, guid);

        if (map == null)
            DestroyGhost();
    }

    private static void TryRestoreMap()
    {
        if (s_currentMap != null) return;

        string guid = SessionState.GetString(k_mapGuidKey, "");
        if (string.IsNullOrEmpty(guid)) return;

        string path = AssetDatabase.GUIDToAssetPath(guid);
        s_currentMap = AssetDatabase.LoadAssetAtPath<MapDataSO>(path);
    }

    /// <summary>
    /// Handles Scene view input, preview rendering, and grid drawing.
    /// </summary>
    private static void OnSceneGUI(SceneView sceneView)
    {
        TryRestoreMap();

        if (s_currentMap == null)
        {
            DestroyGhost();
            return;
        }

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Vector3Int? hoveredCell = GetHoveredCell(sceneView);

        UpdateGhostPreview(hoveredCell);
        DrawGridGizmos();

        if (hoveredCell.HasValue)
            HandleInput(hoveredCell.Value);
    }

    /// <summary>
    /// Determines which grid cell is currently under the mouse cursor.
    /// </summary>
    /// <returns>The hovered cell coordinate, or <c>null</c> if none is found.</returns>
    private static Vector3Int? GetHoveredCell(SceneView sceneView)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        // Always paint on the current layer plane
        float planeY = s_currentLayer;
        Plane gridPlane = new Plane(Vector3.up, new Vector3(0, planeY, 0));

        if (!gridPlane.Raycast(ray, out float distance)) return null;

        Vector3 worldPoint = ray.GetPoint(distance);

        // Clamp to map bounds
        int x = Mathf.FloorToInt(worldPoint.x);
        int z = Mathf.FloorToInt(worldPoint.z);

        if (!s_currentMap.IsValidCoord(x, s_currentLayer, z)) return null;

        return new Vector3Int(x, s_currentLayer, z);
    }

    private static bool s_isDirty = false;
    private static Vector3Int? s_lastPaintedCell;

    /// <summary>
    /// Processes mouse input for placing or erasing blocks.
    /// </summary>
    private static void HandleInput(Vector3Int cell)
    {
        Event e = Event.current;
        if (e.button != 0 || e.alt) return;

        if (e.type == EventType.MouseDown)
        {
            PaintCell(cell);
            s_lastPaintedCell = cell;
            e.Use();
        }
        else if (e.type == EventType.MouseDrag)
        {
            if (s_lastPaintedCell.HasValue)
                PaintLine(s_lastPaintedCell.Value, cell);
            else
                PaintCell(cell);

            s_lastPaintedCell = cell;
            e.Use();
        }
        else if (e.type == EventType.MouseUp)
        {
            if (s_isDirty)
            {
                MapRenderer.RebuildAll(s_currentMap);
                s_isDirty = false;
            }
            s_lastPaintedCell = null;
        }
    }

    private static void PaintCell(Vector3Int cell)
    {
        if (s_isErasing) EraseBlock(cell);
        else PlaceBlock(cell);
    }

    private static void PaintLine(Vector3Int from, Vector3Int to)
    {
        int x0 = from.x, z0 = from.z;
        int x1 = to.x, z1 = to.z;
        int dx = Mathf.Abs(x1 - x0);
        int dz = Mathf.Abs(z1 - z0);
        int sx = x0 < x1 ? 1 : -1;
        int sz = z0 < z1 ? 1 : -1;
        int err = dx - dz;

        while (true)
        {
            PaintCell(new Vector3Int(x0, to.y, z0));
            if (x0 == x1 && z0 == z1) break;
            int e2 = 2 * err;
            if (e2 > -dz) { err -= dz; x0 += sx; }
            if (e2 < dx) { err += dx; z0 += sz; }
        }
    }


    /// <summary>
    /// Places the selected block into the specified cell.
    /// </summary>
    private static void PlaceBlock(Vector3Int cell)
    {
        if (s_selectedBlock == null || !s_currentMap.IsValidCoord(cell.x, cell.y, cell.z)) return;

        Undo.RecordObject(s_currentMap, "Place Block");
        s_currentMap.SetCell(cell.x, cell.y, cell.z, new MapCellData
        {
            BlockId = s_selectedBlock.BlockId,
            IsEmpty = false
        });
        EditorUtility.SetDirty(s_currentMap);
        MapRenderer.SpawnSingle(s_currentMap, cell);
        s_isDirty = true;
    }

    /// <summary>
    /// Clears the specified cell.
    /// </summary>
    private static void EraseBlock(Vector3Int cell)
    {
        if (!s_currentMap.IsValidCoord(cell.x, cell.y, cell.z)) return;

        Undo.RecordObject(s_currentMap, "Erase Block");
        s_currentMap.SetCell(cell.x, cell.y, cell.z, new MapCellData { IsEmpty = true });
        EditorUtility.SetDirty(s_currentMap);
        MapRenderer.RemoveSingle(cell);
        s_isDirty = true;
    }

    /// <summary>
    /// Creates, updates, or destroys the translucent ghost preview object.
    /// </summary>
    private static void UpdateGhostPreview(Vector3Int? cell)
    {
        if (!cell.HasValue || s_selectedBlock == null || s_isErasing)
        {
            DestroyGhost();
            return;
        }

        if (s_ghostInstance == null && s_selectedBlock.Prefab != null)
        {
            s_ghostInstance = Object.Instantiate(s_selectedBlock.Prefab);
            s_ghostInstance.hideFlags = HideFlags.HideAndDontSave;
        }

        if (s_ghostInstance != null)
            s_ghostInstance.transform.position = cell.Value;
    }

    /// <summary>
    /// Draws the grid lines for the active layer in the Scene view.
    /// </summary>
    private static void DrawGridGizmos()
    {
        if (!s_showGrid || s_currentMap == null) return;

        float y = s_currentLayer;
        Handles.color = new Color(1f, 1f, 1f, 0.2f);

        for (float x = 0; x <= s_currentMap.Width; x++)
            Handles.DrawLine(new Vector3(x + 0.5f, y, - 0.5f), new Vector3(x + 0.5f, y, s_currentMap.Depth - 0.5f));

        for (float z = 0; z <= s_currentMap.Depth; z++)
            Handles.DrawLine(new Vector3(0.5f, y, z - 0.5f), new Vector3(s_currentMap.Width + 0.5f, y, z - 0.5f));
    }

    private static List<Material> s_ghostMaterials = new();

    /// <summary>
    /// Removes the current ghost preview instance from the Scene view.
    /// </summary>
    private static void DestroyGhost()
    {
        foreach (var mat in s_ghostMaterials)
            Object.DestroyImmediate(mat);
        s_ghostMaterials.Clear();

        if (s_ghostInstance != null)
            Object.DestroyImmediate(s_ghostInstance);
        s_ghostInstance = null;
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Editor window used to create, select, and edit map assets and their block palette.
/// </summary>
public class MapEditorWindow : EditorWindow
{
    /// <summary>
    /// Session-state key used to remember the currently selected map asset.
    /// </summary>
    private const string k_mapGuidKey = "MapEditor_MapGuid";

    /// <summary>
    /// The map asset currently assigned to the editor.
    /// </summary>
    private MapDataSO m_currentMap;

    /// <summary>
    /// All available block definitions found in the project.
    /// </summary>
    private List<BlockDefinitionSO> m_allBlocks = new();

    /// <summary>
    /// Scroll position for the block palette UI.
    /// </summary>
    private Vector2 m_paletteScroll;

    /// <summary>
    /// The active layer used when painting blocks in the scene.
    /// </summary>
    private int m_selectedLayer = 0;

    /// <summary>
    /// Indicates whether the editor is currently in erase mode.
    /// </summary>
    private bool m_isErasing = false;

    /// <summary>
    /// Indicates whether grid lines are shown in the Scene view.
    /// </summary>
    private bool s_showGrid = true;

    /// <summary>
    /// Opens the map editor window from the Unity menu.
    /// </summary>
    [MenuItem("Tools/Map Editor")]
    public static void OpenWindow() => GetWindow<MapEditorWindow>("Map Editor");

    /// <summary>
    /// Refreshes the available block palette when the window is enabled.
    /// </summary>
    private void OnEnable()
    {
        RefreshBlockPalette();
    }

    /// <summary>
    /// Restores the last edited map asset from the session state when the window gains focus.
    /// </summary>
    private void OnFocus()
    {
        if (m_currentMap == null)
        {
            string guid = SessionState.GetString("MapEditor_MapGuid", "");
            if (string.IsNullOrEmpty(guid)) return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            m_currentMap = AssetDatabase.LoadAssetAtPath<MapDataSO>(path);
            MapEditorTool.SetMap(m_currentMap);
        }
    }

    /// <summary>
    /// Tracks the collapsed/expanded state of the Map section foldout.
    /// </summary>
    private bool m_mapSectionFoldout = true;

    /// <summary>
    /// Tracks the collapsed/expanded state of the Painting Tools section foldout.
    /// </summary>
    private bool m_paintingToolsFoldout = true;

    /// <summary>
    /// Tracks the collapsed/expanded state of the Storage Tools section foldout.
    /// </summary>
    private bool m_storageToolsFoldout = true;

    /// <summary>
    /// Tracks the collapsed/expanded state of the Block Palette section foldout.
    /// </summary>
    private bool m_paletteFoldout = true;

    /// <summary>
    /// Draws the entire editor UI.
    /// </summary>
    private void OnGUI()
    {
        m_mapSectionFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_mapSectionFoldout, "Map");
        if (m_mapSectionFoldout) DrawMapSection();
        EditorGUILayout.EndFoldoutHeaderGroup();

        m_paletteFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_paletteFoldout, "Block Palette");
        if (m_paletteFoldout) DrawPalette();
        EditorGUILayout.EndFoldoutHeaderGroup();

        m_paintingToolsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_paintingToolsFoldout, "Painting Tools");
        if (m_paintingToolsFoldout) DrawPaintingTools();
        EditorGUILayout.EndFoldoutHeaderGroup();

        m_storageToolsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_storageToolsFoldout, "Storage Tools");
        if (m_storageToolsFoldout) DrawStorageTools();
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    /// <summary>
    /// Draws the layer selection and painting mode controls.
    /// </summary>
    private void DrawPaintingTools()
    {
        EditorGUILayout.Space(4);
        DrawLayerControls();
        EditorGUILayout.Space(4);

        bool erasing = GUILayout.Toggle(m_isErasing, "Erase Mode", "Button");
        if (erasing != m_isErasing)
        {
            m_isErasing = erasing;
            MapEditorTool.SetErasing(m_isErasing);
        }

        if (GUILayout.Button(s_showGrid ? "Hide Grid" : "Show Grid"))
        {
            s_showGrid = !s_showGrid;
            MapEditorTool.SetShowGrid(s_showGrid);
            SceneView.RepaintAll();
        }
        EditorGUILayout.Space(4);
    }

    /// <summary>
    /// Draws map storage and management operations.
    /// </summary>
    private void DrawStorageTools()
    {
        EditorGUILayout.Space(4);

        if (GUILayout.Button("Save as Prefab"))
            MapRenderer.SaveAsPrefab();

        if (GUILayout.Button("Clear Map"))
        {
            if (EditorUtility.DisplayDialog("Clear Map", "Remove all blocks?", "Clear", "Cancel"))
                ClearMap();
        }

        EditorGUILayout.Space(4);
    }

    /// <summary>
    /// Draws the map asset selection and map creation controls.
    /// </summary>
    private void DrawMapSection()
    {
        EditorGUILayout.LabelField("Map", EditorStyles.boldLabel);

        var newMap = (MapDataSO)EditorGUILayout.ObjectField("Current Map", m_currentMap, typeof(MapDataSO), false);
        if (newMap != m_currentMap)
            SetCurrentMap(newMap);

        if (GUILayout.Button("New Map"))
            CreateNewMap();

        if (m_currentMap != null && GUILayout.Button("Close Map"))
            CloseMap();
    }

    /// <summary>
    /// Draws the controls used to select the active editing layer.
    /// </summary>
    private void DrawLayerControls()
    {
        EditorGUILayout.LabelField("Layer", EditorStyles.boldLabel);

        // Clamp the slider to valid map layers when a map is loaded.
        int newLayer = EditorGUILayout.IntSlider("Edit Layer", m_selectedLayer, 0, m_currentMap != null ? m_currentMap.Height - 1 : 0);
        if (newLayer != m_selectedLayer)
        {
            m_selectedLayer = newLayer;
            MapEditorTool.SetLayer(m_selectedLayer);
        }
    }

    /// <summary>
    /// Draws the block selection palette.
    /// </summary>
    private void DrawPalette()
    {
        EditorGUILayout.LabelField("Blocks", EditorStyles.boldLabel);
        m_paletteScroll = EditorGUILayout.BeginScrollView(m_paletteScroll, GUILayout.Height(200));

        int columns = Mathf.Max(1, (int)(position.width / 90));

        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            imagePosition = ImagePosition.ImageAbove,
            fixedWidth = 80,
            fixedHeight = 80,
            fontSize = 10,
            wordWrap = true,
        };

        for (int i = 0; i < m_allBlocks.Count; i += columns)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = i; j < Mathf.Min(i + columns, m_allBlocks.Count); j++)
            {
                var block = m_allBlocks[j];
                var content = block.PreviewTexture != null
                    ? new GUIContent(block.DisplayName, block.PreviewTexture)
                    : new GUIContent(block.DisplayName);

                if (GUILayout.Button(content, buttonStyle))
                {
                    MapEditorTool.SetBlock(block);
                    m_isErasing = false;
                    MapEditorTool.SetErasing(false);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Draws tool state controls such as erase mode and map clearing.
    /// </summary>
    private void DrawToolControls()
    {
        EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

        bool erasing = GUILayout.Toggle(m_isErasing, "Erase Mode", "Button");
        if (erasing != m_isErasing)
        {
            m_isErasing = erasing;
            MapEditorTool.SetErasing(m_isErasing);
        }

        if (GUILayout.Button("Clear Map"))
        {
            if (EditorUtility.DisplayDialog("Clear Map", "Remove all blocks?", "Clear", "Cancel"))
                ClearMap();
        }

        if (GUILayout.Button(s_showGrid ? "Hide Grid" : "Show Grid"))
        {
            s_showGrid = !s_showGrid;
            MapEditorTool.SetShowGrid(s_showGrid);
            SceneView.RepaintAll();
        }
    }

    /// <summary>
    /// Reloads every <see cref="BlockDefinitionSO"/> asset found in the project.
    /// </summary>
    private void RefreshBlockPalette()
    {
        m_allBlocks.Clear();
        string[] guids = AssetDatabase.FindAssets("t:BlockDefinitionSO");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            m_allBlocks.Add(AssetDatabase.LoadAssetAtPath<BlockDefinitionSO>(path));
        }
    }

    /// <summary>
    /// Creates a new map asset with default dimensions and saves it to the project.
    /// </summary>
    private void CreateNewMap()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Map", "NewMap", "asset", "Save map asset");
        if (string.IsNullOrEmpty(path)) return;

        var map = CreateInstance<MapDataSO>();
        map.Initialize(10, 4, 10);
        AssetDatabase.CreateAsset(map, path);
        AssetDatabase.SaveAssets();

        m_currentMap = map;
        MapEditorTool.SetMap(m_currentMap);
    }


    /// <summary>
    /// Closes the current map and clears the rendered scene state.
    /// </summary>
    private void CloseMap()
    {
        SetCurrentMap(null);
    }


    /// <summary>
    /// Assigns the active map, persists it in session state, and refreshes the renderer.
    /// </summary>
    /// <param name="map">The map to assign, or <c>null</c> to close the current map.</param>
    private void SetCurrentMap(MapDataSO map)
    {
        m_currentMap = map;
        MapEditorTool.SetMap(m_currentMap);

        string guid = m_currentMap != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_currentMap)) : "";
        SessionState.SetString(k_mapGuidKey, guid);

        MapRenderer.RebuildAll(m_currentMap);
        Repaint();
        SceneView.RepaintAll();
    }


    /// <summary>
    /// Clears every cell in the current map while keeping its dimensions.
    /// </summary>
    private void ClearMap()
    {
        if (m_currentMap == null) return;

        Undo.RecordObject(m_currentMap, "Clear Map");
        m_currentMap.Initialize(m_currentMap.Width, m_currentMap.Height, m_currentMap.Depth);
        EditorUtility.SetDirty(m_currentMap);
        MapRenderer.RebuildAll(m_currentMap);
    }
}
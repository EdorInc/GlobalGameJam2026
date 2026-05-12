using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapDataSO))]
public class MapDataSOEditor : Editor
{
    private int m_newWidth;
    private int m_newHeight;
    private int m_newDepth;

    private void OnEnable()
    {
        var map = (MapDataSO)target;
        m_newWidth = map.Width;
        m_newHeight = map.Height;
        m_newDepth = map.Depth;
    }

    public override void OnInspectorGUI()
    {
        var map = (MapDataSO) target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Map Dimensions", EditorStyles.boldLabel);

        // Edit pending dimensions
        m_newWidth = Mathf.Max(1, EditorGUILayout.IntField("Width", m_newWidth));
        m_newHeight = Mathf.Max(1, EditorGUILayout.IntField("Height", m_newHeight));
        m_newDepth = Mathf.Max(1, EditorGUILayout.IntField("Depth", m_newDepth));

        bool dimensionsChanged = m_newWidth != map.Width || m_newHeight != map.Height || m_newDepth != map.Depth;

        EditorGUILayout.Space();

        // Reveal apply button if adjustments were typed
        GUI.enabled = dimensionsChanged;
        if (GUILayout.Button("Apply New Dimensions", GUILayout.Height(30)))
        {
            Undo.RecordObject(map, "Resize Map");
            map.Resize(m_newWidth, m_newHeight, m_newDepth);
            EditorUtility.SetDirty(map);

            // Auto refresh visually in Scene view if editor system is rendering it
            MapRenderer.RebuildAll(map);
            SceneView.RepaintAll();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox($"Total Cells: {map.Width * map.Height * map.Depth}", MessageType.Info);
    }
}
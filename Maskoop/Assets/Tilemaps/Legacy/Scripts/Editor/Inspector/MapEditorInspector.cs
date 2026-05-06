using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomEditor(typeof(MapEditor))]
public class MapEditorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapEditor mapEditor = (MapEditor)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Grid Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Save"))
        {
            CallMethod(mapEditor, "SaveGrid");
        }

        if (GUILayout.Button("Clear"))
        {
            CallMethod(mapEditor, "ClearGrid");
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Map Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate"))
        {
            CallMethod(mapEditor, "GenerateMap");
        }

        if (GUILayout.Button("Save"))
        {
            CallMethod(mapEditor, "SaveMap");
        }

        if (GUILayout.Button("Clear"))
        {
            CallMethod(mapEditor, "ClearMap");
        }
    }

    void CallMethod(MapEditor targetScript, string methodName)
    {
        MethodInfo method = typeof(MapEditor).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

        if (method != null)
        {
            method.Invoke(targetScript, null);
        }
        else
        {
            Debug.LogError($"Method {methodName} not found.");
        }
    }
}
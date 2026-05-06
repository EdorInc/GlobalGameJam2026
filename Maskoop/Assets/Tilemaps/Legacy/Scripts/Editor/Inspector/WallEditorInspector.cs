using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WallEditor))]
public class WallEditorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WallEditor script = (WallEditor)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Adjust"))
        {
            script.Adjust();
        }
    }
}
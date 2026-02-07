using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class WallManager : MonoBehaviour
{

    [SerializeField, Min(1)] private int wallHeight = 2;

    private BuildWall[] walls;

    private void OnEnable()
    {
        RebuildAll();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorApplication.delayCall += RebuildAll;
    }
#endif

    public void RebuildAll()
    {
        if (!this) return;

        walls = FindObjectsByType<BuildWall>(FindObjectsSortMode.None);

        foreach (var wall in walls)
        {
            wall.WallHeight = wallHeight;
        }
    }
}
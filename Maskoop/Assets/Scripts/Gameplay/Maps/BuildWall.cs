using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class BuildWall : MonoBehaviour
{
    [SerializeField]
    private GameObject wallPrefab;

    [SerializeField, Min(1)]
    private int wallHeight = 3;

    [SerializeField, Min(1)]
    private int wallWidth = 1;

    public int WallHeight
    {
        get => wallHeight;
        set
        {
            if (wallHeight != value)
            {
                wallHeight = Mathf.Max(1, value);
                Rebuild();
            }
        }
    }

    public int WallWidth
    {
        get => wallWidth;
        set
        {
            if (wallWidth != value)
            {
                wallWidth = Mathf.Max(1, value);
                Rebuild();
            }
        }
    }

#if UNITY_EDITOR
    private bool rebuildQueued;
#endif

    public void Rebuild()
    {
#if UNITY_EDITOR
        if (rebuildQueued) return;
        rebuildQueued = true;
        EditorApplication.delayCall += RebuildSafe;
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Rebuild();
    }

    private void RebuildSafe()
    {
        rebuildQueued = false;

        if (!this || !wallPrefab) return;

        wallPrefab.transform.localScale = new Vector3(wallWidth, wallHeight, 1);
    }
#endif
}

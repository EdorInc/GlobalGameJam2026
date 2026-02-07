using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class BuildWall : MonoBehaviour
{
    [SerializeField]
    private GameObject wallPrefab;

    [SerializeField, Min(1)]
    private int wallHeight = 3;

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
        if (PrefabUtility.IsPartOfPrefabAsset(this)) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < wallHeight; i++)
        {
            var segment = Instantiate(wallPrefab, transform);
            segment.transform.localPosition = Vector3.up * i;
        }
    }
#endif
}

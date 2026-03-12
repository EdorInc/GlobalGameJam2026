using UnityEngine;

public class WallEditor : MonoBehaviour
{
    [Header("Custom Settings")]
    [SerializeField, Min(1)] private int wallHeight = 3;

    [ContextMenu("Adjust")]
    public void Adjust()
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (!child.CompareTag("Wall"))
                continue;

            if (child.parent != null && child.parent.CompareTag("Wall"))
                continue;

            child.localScale = new Vector3(child.localScale.x, wallHeight, child.localScale.z);
        }
    }
}
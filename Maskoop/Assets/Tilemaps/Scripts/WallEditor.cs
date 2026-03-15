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
            Transform target = child;

            bool isDoor = child.TryGetComponent(out Door door) && door.GetType() == typeof(Door);
            if (isDoor)
            {
                if (child.parent == null)
                    continue;

                target = child.parent; // operate on the immediate parent
            }

            bool isWall = target.CompareTag("Wall");

            if (!isWall && !isDoor)
                continue;

            if (target.parent != null && target.parent.CompareTag("Wall"))
                continue;

            target.localScale = new Vector3(
                target.localScale.x,
                wallHeight,
                target.localScale.z
            );
        }
    }
}
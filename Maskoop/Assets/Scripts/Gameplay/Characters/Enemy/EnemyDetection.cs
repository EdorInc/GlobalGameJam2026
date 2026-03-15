using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("Vision parameters")]
    [SerializeField] private float outerConeDistance = 5;
    [SerializeField] private float innerConeDistance = 3;
    [SerializeField] private float outerConeAngle = 90;
    [SerializeField] private float innerConeAngle = 60;

    private void OnDrawGizmos()
    {

        Vector3 start = transform.position + Vector3.up;

        Gizmos.color = Color.yellow;

        Vector3 left = Quaternion.Euler(0, -outerConeAngle / 2,0) * transform.forward * outerConeDistance;
        Vector3 right = Quaternion.Euler(0, outerConeAngle / 2,0) * transform.forward * outerConeDistance;

        Gizmos.DrawLine(start, start + left);
        Gizmos.DrawLine(start, start + right);

        Gizmos.color = Color.red;

        left = Quaternion.Euler(0, -innerConeAngle / 2, 0) * transform.forward * innerConeDistance;
        right = Quaternion.Euler(0, innerConeAngle / 2, 0) * transform.forward * innerConeDistance;

        Gizmos.DrawLine(start, start + left);
        Gizmos.DrawLine(start, start + right);
    }

}

using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("Vision parameters")]
    [SerializeField] private float outerConeDistance = 5;
    [SerializeField] private float innerConeDistance = 3;
    [SerializeField] private float outerConeAngle = 90;
    [SerializeField] private float innerConeAngle = 60;
    [SerializeField] private float innerSphereRadius = 4;
    [SerializeField] private LayerMask targetMask;

    public bool playerInSight = false;
    private GameObject playerTarget;
    private void Update()
    {
        Vector3 origin = transform.position + Vector3.up;
        if (playerInSight)
        {
            Vector3 dirToTarget = (playerTarget.transform.position - origin).normalized;

            float angle = Vector3.Angle(transform.forward, dirToTarget);
            float distance = Vector3.Distance(origin, playerTarget.transform.position);


            if ((angle >= outerConeAngle / 2f || distance >= outerConeDistance ) && distance >= innerSphereRadius)
            {

                playerInSight = false;
                playerTarget = null;
            }
        }
        else
        {
            
            Collider[] hits = Physics.OverlapSphere(origin, outerConeDistance, targetMask);

            foreach (Collider hit in hits)
            {
                if (!hit.CompareTag("Player"))
                {
                    continue;
                }
                Vector3 dirToTarget = (hit.transform.position - origin).normalized;

                float angle = Vector3.Angle(transform.forward, dirToTarget);


                if (angle <= innerConeAngle / 2f)
                {
                    float distance = Vector3.Distance(origin, hit.transform.position);

                    if (distance <= innerConeDistance)
                    {
                        Debug.Log("INNER detection: " + hit.name);
                        playerInSight = true;
                        playerTarget = hit.gameObject;
                    }
                }
            }
        }
    }

    public Vector3 GetPlayerLocation()
    {
        if (playerTarget)
        {
            return playerTarget.transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }


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

        if(playerTarget)
            Gizmos.DrawWireCube(playerTarget.transform.position, new Vector3(1, 1, 1));
    }



}

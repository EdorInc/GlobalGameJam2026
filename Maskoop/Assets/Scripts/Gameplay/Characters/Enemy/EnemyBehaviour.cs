using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public NavMeshManager NavMeshManager;

    EnemyInputController enemyInput;

    EnemyDetection enemyDetection;

    private Vector3 targetPosition;

    private List<Vector3> currentPath;

    private int currentPoint = 0;
    private bool waiting = false;


    [Header("Patrol Area")]
    public Transform patrolCenter;
    public float patrolRadius;
    public float minDistance = 3f; 
    public float waitTime = 1.5f;
    [Header("ThrowParameters")]
    public float timeToThrow = 1;

    [Header("Not design attributes (if you modify this values the enemy could stop working)")]
    public float distanceToRecalculate = 5;
    public float distanceToChase = 1;
    public float pathRecalculationCooldown = 0.5f;
    public float distanceToGrab = 0.6f;
    public float ignorePlayerTimeAfterThrow = 1.5f;


    private float lastPathRecalculationTime = -Mathf.Infinity;
    private Vector3 finalDestination;
    private float ignorePlayerUntil = 0f;
    private CharacterStateController stateController; 
    private void Start()
    {
        enemyInput = GetComponent<EnemyInputController>();
        enemyDetection = GetComponent<EnemyDetection>();
        stateController = GetComponent<CharacterStateController>();
        targetPosition = transform.position;
    }

    private void OnEnable()
    {
        EventManager.OnNavMeshUpdate += UpdateNavMeshPath;
    }

    private void OnDisable()
    {
        EventManager.OnNavMeshUpdate -= UpdateNavMeshPath;
    }

    private void EndThrow()
    {
        enemyInput.OnThrow(false);
    }
    void FixedUpdate()
    {
        
        if (waiting)
        {
            GoToPosition(transform.position);
            return;
        }

        if(stateController.GetHeldObject() != null)
        {
            
            if (Vector3.Distance(targetPosition, transform.position) < 0.1)
            {
                enemyInput.OnThrow(true);
                ignorePlayerUntil = Time.time + ignorePlayerTimeAfterThrow;
                GoToPosition(transform.position);
                Invoke(nameof(EndThrow), timeToThrow);
            }
            else
            {
                GoToPosition(targetPosition);
            }
            return;
        }
        else if (enemyDetection.playerInSight && Time.time > ignorePlayerUntil)
        {
            waiting = false;

            float distanceToPlayer = Vector3.Distance(transform.position, enemyDetection.GetPlayerLocation());

            Debug.Log("Distance = " + distanceToPlayer);

            if (distanceToPlayer < distanceToGrab)
            {
                targetPosition = NavMeshManager.FindNearestEdge(transform.position);
                enemyInput.OnGrab();
                GoToPosition(transform.position);
                return;
            }
            else if (distanceToPlayer < distanceToChase)
            {
                GoToPosition(enemyDetection.GetPlayerLocation());               
                return;
            }

            if (finalDestination == Vector3.zero)
            {
                lastPathRecalculationTime = Time.time;

                finalDestination = enemyDetection.GetPlayerLocation();

                currentPath = NavMeshManager.FindPath(
                    NavMeshManager.WorldToTile(transform.position),
                    NavMeshManager.WorldToTile(finalDestination)
                );

                if (currentPath != null && currentPath.Count > 0)
                {
                    currentPoint = 0;
                    targetPosition = currentPath[currentPoint];
                }

                return;
            }

            float distanceFromTarget = Vector3.Distance(finalDestination, enemyDetection.GetPlayerLocation());


            if (distanceFromTarget > distanceToRecalculate && Time.time > lastPathRecalculationTime + pathRecalculationCooldown)
            {
                lastPathRecalculationTime = Time.time;

                finalDestination = enemyDetection.GetPlayerLocation();

                currentPath = NavMeshManager.FindPath(
                    NavMeshManager.WorldToTile(transform.position),
                    NavMeshManager.WorldToTile(finalDestination)
                );

                if (currentPath != null && currentPath.Count > 0)
                {
                    currentPoint = 0;
                    targetPosition = currentPath[currentPoint];
                }
            }



        }
        else
        {
            finalDestination = Vector3.zero;
        }

        if (Vector3.Distance(targetPosition, transform.position) < 0.1)
        {
            if ((currentPath == null || currentPoint >= currentPath.Count) && !enemyDetection.playerInSight)
            {
                waiting = true;
                Invoke(nameof(ChooseNextPatrolPoint), waitTime);
                return;
            }
            else
            {
                targetPosition = currentPath[currentPoint];
                currentPoint++;
            }

        }
        GoToPosition(targetPosition);

    }

    void ChooseNextPatrolPoint()
    {
        Vector3 newTarget = GetRandomPatrolPoint();

        // recalcular path
        currentPath = NavMeshManager.FindPath(
            NavMeshManager.WorldToTile(transform.position),
            NavMeshManager.WorldToTile(newTarget));

        if (currentPath != null && currentPath.Count > 0)
        {
            currentPoint = 0;
            targetPosition = currentPath[currentPoint];
        }
        else
        {
            targetPosition = transform.position;
        }
        waiting = false;
    }

    Vector3 GetRandomPatrolPoint()
    {
        Vector3 point;
        int tries = 0;
        do
        {
            point = RandomPointInCircle();
            tries++;
        }
        while ((Vector3.Distance(point, transform.position) < minDistance) && tries < 20);

        // asegurarse que sea tile caminable
        Vector2 tile = NavMeshManager.WorldToTile(point);
        if (!NavMeshManager.IsTileWalkable(tile))
            return transform.position;

        return point;
    }

    private Vector3 RandomPointInCircle()
    {
        float randomRadius = Random.value * patrolRadius;
        float theta = 2 * Mathf.PI * Random.value;

        Vector3 position = patrolCenter.position + randomRadius * new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta));

        return position;
    }

    void UpdateNavMeshPath(NavMeshManager manager)
    {
        /*
        if(manager == NavMeshManager)
        {
            if (currentPath != null && currentPath.Count > 0)
            {
                Vector3 goal = currentPath[currentPath.Count - 1];

                currentPath = NavMeshManager.FindPath(
                    NavMeshManager.WorldToTile(transform.position),
                    NavMeshManager.WorldToTile(goal)
                );

                currentPoint = 0;
                Debug.Log("CAMBIANDO EL PATH");
            }
        }
        */
    }
    void GoToPosition(Vector3 worldPosition)
    {
        Vector3 actualPosition = transform.position;

        Vector3 distance = worldPosition - actualPosition;

        Vector3 direction = distance.normalized;

        Vector2 inputForMovement = new Vector2(direction.x, direction.z);

        enemyInput.OnMove(inputForMovement);
    }

    void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        Gizmos.color = Color.purple;

        for (int i = 0; i < currentPath.Count; i++)
        {
            Gizmos.DrawSphere(currentPath[i], 0.15f);

            if (i < currentPath.Count - 1)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(patrolCenter.position, patrolRadius);
    }

}

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
    private int currentPatrolIndex = 0;
    private bool wasChasingPlayer = false;

    [Header("Patrol")]
    public List<Transform> patrolPoints;
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
    private string currentDebugState = "";
    private bool switchingToPath = false;
    private void Start()
    {
        enemyInput = GetComponent<EnemyInputController>();
        enemyDetection = GetComponent<EnemyDetection>();
        stateController = GetComponent<CharacterStateController>();
        targetPosition = transform.position;
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            ReturnToClosestPatrolPoint();
        }
    }
    private void SetDebugState(string newState)
    {
        if (currentDebugState == newState)
            return;

        currentDebugState = newState;
        Debug.Log($"[Enemy] {gameObject.name} -> {newState}");
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
            SetDebugState("Waiting");
            GoToPosition(transform.position);
            return;
        }

        if(stateController.GetHeldObject() != null)
        {
            SetDebugState("Carrying Player");
            if (HorizontalDistanceSqr(targetPosition, transform.position) < 0.1)
            {
                SetDebugState("Throwing");
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
            SetDebugState("Chasing Player");

            if (!wasChasingPlayer)
            {
                finalDestination = Vector3.zero;
            }

            waiting = false;
            wasChasingPlayer = true;
            float distanceToPlayer = Vector3.Distance(transform.position, enemyDetection.GetPlayerLocation());

            if (distanceToPlayer < distanceToGrab)
            {
                SetDebugState("Grabbing Player");
                targetPosition = NavMeshManager.FindNearestEdge(transform.position);
                enemyInput.OnGrab();
                GoToPosition(transform.position);
                return;
            }
            else if (distanceToPlayer < distanceToChase)
            {
                SetDebugState("Direct Chase");
                switchingToPath = true;
                GoToPosition(enemyDetection.GetPlayerLocation());               
                return;
            }

            if (finalDestination == Vector3.zero || switchingToPath)
            {
                switchingToPath = false;
                SetDebugState("Calculating Chase Path");
                lastPathRecalculationTime = Time.time;

                finalDestination = enemyDetection.GetPlayerLocation();

                currentPath = NavMeshManager.FindPath(
                    NavMeshManager.WorldToTile(transform.position),
                    NavMeshManager.WorldToTile(finalDestination)
                );

                if (currentPath != null && currentPath.Count > 0)
                {
                    currentPoint = 0;
                    Vector3 dirToFirst = (currentPath[0] - transform.position).normalized;
                    Vector3 dirToPlayer = (enemyDetection.GetPlayerLocation() - transform.position).normalized;

                    if (Vector3.Dot(dirToFirst, dirToPlayer) < 0)
                    {
                        if (currentPath.Count > 1)
                            currentPoint = 1;
                    }

                    targetPosition = currentPath[currentPoint];
                }
                else
                {
                    finalDestination = Vector3.zero;
                }

                return;
            }

            float distanceFromTarget = HorizontalDistanceSqr(finalDestination, enemyDetection.GetPlayerLocation());


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
            if (wasChasingPlayer)
            {
                SetDebugState("Returning To Patrol");
                ReturnToClosestPatrolPoint();
                wasChasingPlayer = false;
            }

            finalDestination = Vector3.zero;
        }

        if (HorizontalDistanceSqr(targetPosition, transform.position) < 0.1)
        {
            if ((currentPath == null || currentPoint >= currentPath.Count) && !enemyDetection.playerInSight)
            {
                waiting = true;
                Invoke(nameof(ChooseNextPatrolPoint), waitTime);
                return;
            }
            else
            {
                if (currentPoint < currentPath.Count)
                {
                    targetPosition = currentPath[currentPoint];
                    currentPoint++;
                }
            }

        }
        GoToPosition(targetPosition);
        if (!enemyDetection.playerInSight && stateController.GetHeldObject() == null && !waiting)
        {
            SetDebugState($"Patrolling -> Point {currentPatrolIndex}");
        }
    }

    private float HorizontalDistanceSqr(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return dx * dx + dz * dz;
    }

    void ChooseNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            waiting = false;
            return;
        }

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Count)
            currentPatrolIndex = 0;

        currentPath = NavMeshManager.FindPath(
            NavMeshManager.WorldToTile(transform.position),
            NavMeshManager.WorldToTile(patrolPoints[currentPatrolIndex].position)
        );

        if (currentPath != null && currentPath.Count > 0)
        {
            currentPoint = 0;
            targetPosition = currentPath[currentPoint];
        }

        waiting = false;
    }

    private void ReturnToClosestPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
            return;

        float closestDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < patrolPoints.Count; i++)
        {
            float distance = Vector3.Distance(
                transform.position,
                patrolPoints[i].position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        currentPatrolIndex = closestIndex;

        currentPath = NavMeshManager.FindPath(
            NavMeshManager.WorldToTile(transform.position),
            NavMeshManager.WorldToTile(patrolPoints[currentPatrolIndex].position)
        );

        if (currentPath != null && currentPath.Count > 0)
        {
            currentPoint = 0;
            targetPosition = currentPath[currentPoint];
        }
    }

    void UpdateNavMeshPath(NavMeshManager manager)
    {
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

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            currentDebugState);
#endif
    }

    void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
            return;

        Gizmos.color = Color.blue;

        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);

            if (i < patrolPoints.Count - 1 && patrolPoints[i + 1] != null)
            {
                Gizmos.DrawLine(
                    patrolPoints[i].position,
                    patrolPoints[i + 1].position);
            }
        }

        if (patrolPoints.Count > 1 &&
            patrolPoints[0] != null &&
            patrolPoints[patrolPoints.Count - 1] != null)
        {
            Gizmos.DrawLine(
                patrolPoints[patrolPoints.Count - 1].position,
                patrolPoints[0].position);
        }
    }

}

using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public NavMeshManager NavMeshManager;


    EnemyInputController enemyInput;

    private Vector3 targetPosition;



    private void Start()
    {
        enemyInput = GetComponent<EnemyInputController>();
        targetPosition = NavMeshManager.GetRandomPointInMap();
    }
    void Update()
    {
        GoToPosition(targetPosition);

        if(Vector3.Distance(targetPosition, transform.position) < 0.1)
        {
            targetPosition = NavMeshManager.GetRandomPointInMap();
        }
    }


    void GoToPosition(Vector3 worldPosition)
    {
        Vector3 actualPosition = transform.position;

        Vector3 distance = worldPosition - actualPosition;

        Vector3 direction = distance.normalized;

        Vector2 inputForMovement = new Vector2(direction.x, direction.z);

        enemyInput.OnMove(inputForMovement);
    }
}

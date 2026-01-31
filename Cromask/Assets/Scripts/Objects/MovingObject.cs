using UnityEngine;

public class MovingObject : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private Transform pointA;
    [SerializeField]
    private Transform pointB;

    private Vector3 target;

    private void Start()
    {
        target = pointB.position;
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            target = target == pointA.position ? pointB.position : pointA.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Colision");
        other.gameObject.transform.parent = transform;
    }
}

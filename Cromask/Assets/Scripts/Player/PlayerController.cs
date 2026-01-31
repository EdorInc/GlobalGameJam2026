using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -6.81f;

    private CharacterController characterController;
    private Vector2 moveDirection;
    private Vector3 platformVelocity = Vector3.zero;
    private Vector3 verticalVelocity;
    private bool attached = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }
    }

    private void Update()
    {
        CheckPlatform();

        Vector3 horizontalMove = new Vector3(moveDirection.x, 0f, moveDirection.y) * moveSpeed;

        Vector3 move = horizontalMove + platformVelocity;

       
        if (attached)
        {
            if (platformVelocity.y > 0)
            {
                verticalVelocity.y = platformVelocity.y;
            }
            else
            {
                verticalVelocity.y = -2f;
            }
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        move += verticalVelocity;

        characterController.Move(move * Time.deltaTime);

        if (moveDirection != Vector2.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0f, moveDirection.y));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void OnMove(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    private void CheckPlatform()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, characterController.height / 2f + 0.2f))
        {
            if (hit.collider.CompareTag("MovingPlatform"))
            {
                MovingObject platform = hit.collider.GetComponent<MovingObject>();
                if (platform != null)
                {
                    platformVelocity = platform.GetVelocity();
                    attached = true;
                    return; 
                }
            }
        }

        platformVelocity = Vector3.zero;
        attached = false;
    }
}
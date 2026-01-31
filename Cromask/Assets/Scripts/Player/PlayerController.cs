using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    private CharacterController characterController;
    private Vector2 moveDirection;

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
        // Calcular y aplicar movimiento SIEMPRE (incluso si es Vector3.zero)
        Vector3 move = new Vector3(moveDirection.x, 0f, moveDirection.y) * moveSpeed;
        characterController.SimpleMove(move);

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
}

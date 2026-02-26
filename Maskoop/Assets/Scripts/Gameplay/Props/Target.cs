using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Connection Settings")]
    [Tooltip("Channel to use to connect to doors. Doors need to have the same channel be send a message")]
    [SerializeField] private int channel = 1;

    private bool hasBeenActivated = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rock") && !hasBeenActivated)
        {
            hasBeenActivated = true;
            EventManager.OnButtonPressed?.Invoke(channel);
        }
    }
}

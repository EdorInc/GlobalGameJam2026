using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    private PauseManager pauseManager;

    private void Start()
    {
        pauseManager = PauseManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() == null) return;
        Debug.Log("Checkpoint reached at: " + transform.position);
        pauseManager.SetCheckpoint(transform);
    }
}

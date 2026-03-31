using UnityEngine;

public class WinCondition : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("A player reached the goal!");

        EventManager.OnVictory?.Invoke();
    }
}

using System.Collections.Generic;
using UnityEngine;

public class WinCondition : MonoBehaviour
{
    [Header("Condition Settings")]
    [Tooltip("Whether all players need to reach the goal to win, or just one of them.")]
    [SerializeField] private bool allPlayerNeeded = true;

    private readonly HashSet<GameObject> playersAtGoal = new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playersAtGoal.Add(other.gameObject);
        Debug.Log($"Player '{other.gameObject.name}' reached the goal!");

        if (allPlayerNeeded)
        {
            // Find all active players in the scene
            var allPlayers = GameObject.FindGameObjectsWithTag("Player");
            int totalPlayers = allPlayers.Length;

            Debug.Log($"Players at goal: {playersAtGoal.Count} / {totalPlayers}");

            if (playersAtGoal.Count >= totalPlayers && totalPlayers > 0)
            {
                Debug.Log("All players reached the goal!");
                EventManager.OnVictory?.Invoke();
            }
        }
        else
        {
            Debug.Log("Victory triggered by a single player!");
            EventManager.OnVictory?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Remove player if they leave the goal area
        if (playersAtGoal.Remove(other.gameObject))
        {
            Debug.Log($"Player '{other.gameObject.name}' left the goal area.");
        }
    }
}

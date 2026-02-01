using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    private Dictionary<string, Transform> spawnPoints = new Dictionary<string, Transform>();

    [Header("Spawn Points")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    [SerializeField] private Transform red;
    [SerializeField] private Transform green;
    [SerializeField] private Transform blue;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            switch (child.name)
            {
                case "Player1Spawn":
                    player1 = child;
                    break;

                case "Player2Spawn":
                    player2 = child;
                    break;

                case "RedSpawn":
                    red = child;
                    break;

                case "GreenSpawn":
                    green = child;
                    break;

                case "BlueSpawn":
                    blue = child;
                    break;
            }
        }
    }

    void Start()
    {
        if (SpawnManager.Instance == null)
        {
            Debug.LogError("SpawnManager not found in scene!");
            return;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (player1 != null && player2 != null)
        {
            SpawnManager.Instance.RegisterPlayerSpawnPoints(player1, player2);
        }
        else
        {
            Debug.LogWarning("Missing player spawn points in Spawner.");
        }

        if (red != null && green != null && blue != null)
        {
            SpawnManager.Instance.RegisterMasksSpawnPoint(red, green, blue);
        }
        else
        {
            Debug.LogWarning("Missing mask spawn points in Spawner.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
            SpawnManager.Instance.ForceRespawnAll();
    }
}

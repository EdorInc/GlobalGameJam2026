using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Singleton instance
    public static SpawnManager Instance { get; private set; }

    // Spawn points (empty GameObjects)
    [Header("Spawn Points")]
    [SerializeField]
    private Transform Player1SpawnPoint;
    [SerializeField]
    private Transform Player2SpawnPoint;
    [SerializeField]
    private Transform RedSpawnPoint;
    [SerializeField]
    private Transform BlueSpawnPoint;
    [SerializeField]
    private Transform GreenSpawnPoint;

    private void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Optional: persist across scenes
        // DontDestroyOnLoad(gameObject);
    }

    public void RegisterPlayerSpawnPoints(Transform player1SpawnPoint, Transform player2SpawnPoint)
    {
        Player1SpawnPoint = player1SpawnPoint;
        Player2SpawnPoint = player2SpawnPoint;
    }

    public void RegisterMasksSpawnPoint(Transform redSpawnPoint, Transform greenSpawnPoint, Transform blueSpawnPoint)
    {
        RedSpawnPoint = redSpawnPoint;
        GreenSpawnPoint = greenSpawnPoint;
        BlueSpawnPoint = blueSpawnPoint;
    }

    public void ForceRespawnAll()
    {
        ReferenceManager refs = ReferenceManager.Instance;

        GameObject player1 = refs.GetPlayerOne();
        player1.GetComponent<GrabAction>().ThrowObject();
        player1.GetComponent<EquipAction>().UnEquip();
        player1.GetComponent<GrabAction>().ThrowObject();

        GameObject player2 = refs.GetPlayerTwo();
        player2.GetComponent<GrabAction>().ThrowObject();
        player2.GetComponent<EquipAction>().UnEquip();
        player2.GetComponent<GrabAction>().ThrowObject();

        Reset[] resetComponents = Object.FindObjectsByType<Reset>(FindObjectsSortMode.None);

        foreach (Reset reset in resetComponents)
        {
            reset.ForceRespawn();
        }

        ForceRespawn(player1, Player1SpawnPoint);
        ForceRespawn(player2, Player2SpawnPoint);

        ForceRespawn(refs.GetRedMask(), RedSpawnPoint);
        ForceRespawn(refs.GetGreenMask(), GreenSpawnPoint);
        ForceRespawn(refs.GetBlueMask(), BlueSpawnPoint);
    }

    private void ForceRespawn(GameObject target, Transform spawnPoint)
    {
        if (target == null || spawnPoint == null)
            return;

        ResetMask reset = target.GetComponent<ResetMask>();
        if (reset != null)
        {
            reset.ForceRespawn(spawnPoint);
        }

        Respawn respawn = target.GetComponent<Respawn>();
        if (respawn != null)
        {
            respawn.ForceRespawn(spawnPoint);
        }
    }
}
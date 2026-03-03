using System.Collections.Generic;
using UnityEngine;

public class RockThrower : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] protected Transform spawnPosition;

    [Header("Spawn Settings")]
    [SerializeField] float launchForce = 10;
    [SerializeField] protected GameObject rockPrefab;
    [SerializeField] float spawnDelay = 1f;

    private float currentTimer = 0;


    private void Update()
    {
        currentTimer += Time.deltaTime;
        if(currentTimer > spawnDelay)
        {
            currentTimer = 0;
            SpawnRock();
        }
    }
    private void SpawnRock()
   {
        GameObject rockSpawned = Instantiate(rockPrefab, spawnPosition.position, Quaternion.identity);

        Vector3 direction = (spawnPosition.position - transform.position ).normalized;

        rockSpawned.GetComponent<Rigidbody>().AddForce(direction * launchForce, ForceMode.Impulse);
   }
}

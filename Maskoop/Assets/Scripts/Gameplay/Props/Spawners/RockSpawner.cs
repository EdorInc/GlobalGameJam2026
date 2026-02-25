using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform spawnPosition;

    private GameObject rockSpawned;

    void Start()
    {
        SpawnRock();
    }

    // Update is called once per frame
    void Update()
    {
        if( rockSpawned == null)
        {
            SpawnRock();
        }
    }

    private void SpawnRock()
    {
        rockSpawned = Instantiate(rockPrefab, spawnPosition.position, Quaternion.identity);
    }
}

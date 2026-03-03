using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] protected GameObject rockPrefab;
    [SerializeField] protected Transform spawnPosition;

    protected GameObject rockSpawned;

    void Start()
    {
        SpawnRock();
    }

    protected virtual void SpawnRock()
    {
        if(rockSpawned == null)
        {
            rockSpawned = Instantiate(rockPrefab, spawnPosition.position, Quaternion.identity);
            rockSpawned.GetComponent<Breakable>().SetSpawner(this);
        }
        else
        {
            rockSpawned.transform.position = spawnPosition.position;
        }
    }

    public void DestroyRock(float respawnDelay = 0.0f)
    {
        Invoke(nameof(DestroyAndRespawnRock), respawnDelay);
    }

    private void DestroyAndRespawnRock()
    {
        if (rockSpawned == null)
            return;

        Destroy(rockSpawned);
        rockSpawned = null;

        SpawnRock();
    }
}

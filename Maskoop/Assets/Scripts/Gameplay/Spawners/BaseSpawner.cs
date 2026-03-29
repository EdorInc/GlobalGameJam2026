using UnityEngine;

public class BaseSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Spawner Settings")]
    [SerializeField] protected GameObject objectPrefab;
    [SerializeField] protected Transform spawnPosition;

    protected GameObject objectSpawned;

    void Start()
    {
        SpawnObject();
    }

    protected virtual void SpawnObject()
    {
        if (objectSpawned == null)
        {
            InitializeObject();
        }
        else
        {
            objectSpawned.transform.position = spawnPosition.position;
        }
    }

    protected virtual void InitializeObject()
    {
        // This method can be overridden by derived classes to initialize the spawned object if needed.
        objectSpawned = Instantiate(objectPrefab, spawnPosition.position, Quaternion.identity);
        objectSpawned.GetComponent<Breakable>()?.SetSpawner(this);
        objectSpawned.GetComponent<Respawn>()?.SetSpawner(this);
    }

    public void DestroyObject(float respawnDelay = 0.0f)
    {
        Invoke(nameof(DestroyAndRespawnObject), respawnDelay);
    }

    private void DestroyAndRespawnObject()
    {
        if (objectSpawned == null)
            return;

        Destroy(objectSpawned);
        objectSpawned = null;

        SpawnObject();
    }
}


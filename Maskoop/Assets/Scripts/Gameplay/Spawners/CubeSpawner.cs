using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Transform spawnPosition;

    private GameObject cubeSpawned;

    void Start()
    {
        SpawnCube();
    }

    private void SpawnCube()
    {
        if(cubeSpawned == null)
        {
            cubeSpawned = Instantiate(cubePrefab, spawnPosition.position, Quaternion.identity);
            cubeSpawned.GetComponent<Respawn>().SetSpawner(this);
        }
        else
        {
            cubeSpawned.transform.position = spawnPosition.position;
        }
    }

    public void DestroyCube(float respawnDelay = 0.0f)
    {
        Invoke(nameof(DestroyAndRespawnCube), respawnDelay);
    }

    private void DestroyAndRespawnCube()
    {
        if (cubeSpawned == null)
            return;

        Destroy(cubeSpawned);
        cubeSpawned = null;

        SpawnCube();
    }
}

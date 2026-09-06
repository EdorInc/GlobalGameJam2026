using System.Collections.Generic;
using UnityEngine;

public class BaseSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] protected GameObject objectPrefab;
    [SerializeField] protected Transform spawnPosition;
    public bool isMaskSpawner;

    protected GameObject objectSpawned;

    private static readonly List<BaseSpawner> maskSpawners = new List<BaseSpawner>();
    private static BaseSpawner maskHolder;

    protected virtual void OnEnable()
    {
        if (isMaskSpawner)
            maskSpawners.Add(this);
    }

    protected virtual void OnDisable()
    {
        if (isMaskSpawner)
            maskSpawners.Remove(this);
    }

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
        objectSpawned = Instantiate(objectPrefab, spawnPosition.position, Quaternion.identity);
        objectSpawned.GetComponent<Breakable>()?.SetSpawner(this);
        objectSpawned.GetComponent<Respawn>()?.SetSpawner(this);
        objectSpawned.GetComponent<BaseMask>()?.SetSpawner(this);
    }

    public void DestroyObject(float respawnDelay = 0.0f)
    {
        Invoke(nameof(DestroyAndRespawnObject), respawnDelay);
    }

    protected void DestroyAndRespawnObject()
    {
        if (objectSpawned == null)
            return;

        Destroy(objectSpawned);
        objectSpawned = null;
        OnMaskReturned();
        SpawnObject();
    }

    public void OnMaskTaken()
    {
        if (!isMaskSpawner || maskHolder != null)
            return;

        maskHolder = this;

        foreach (var spawner in maskSpawners)
        {
            if (spawner == this) continue;
            spawner.HideMask();
        }
    }

    public void OnMaskReturned()
    {
        if (maskHolder != this)
            return;

        maskHolder = null;

        foreach (var spawner in maskSpawners)
        {
            if (spawner == this) continue;
            spawner.ShowMask();
        }
    }

    private void HideMask()
    {
        if (objectSpawned != null)
        {
            Destroy(objectSpawned);
            objectSpawned = null;
        }
    }

    private void ShowMask()
    {
        if (objectSpawned == null)
            InitializeObject();
    }
}
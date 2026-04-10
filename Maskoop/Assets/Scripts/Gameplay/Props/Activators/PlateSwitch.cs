using System.Collections.Generic;
using UnityEngine;

public class PlateSwitch : BaseSwitch
{
    [Header("Activation Settings")]
    [Tooltip("Whether the pressure plate can only be activated by the player.")]
    [SerializeField] protected bool playerExclusive = true;
    [Tooltip("")]
    [SerializeField] protected LayerMask includedLayersInCheck = ~0;

    [Header("Apparience Settings")]
    [Tooltip("Provisinal material to use when the target is activated.")]
    [SerializeField] protected Material activatedMaterial;
    [Tooltip("Provisinal width to use when the target is activated.")]
    [SerializeField] protected float activatedWidth = 0.1f;

    protected Material deactivatedMaterial;
    protected float deactivatedWidth;

    private readonly HashSet<GameObject> objectsOnPlate = new HashSet<GameObject>();

#if UNITY_EDITOR
    [Header("Debug Settings")]
    [Tooltip("List of objects currently detected on the plate, for debugging purposes.")]
    [SerializeField] private List<GameObject> debugObjectsOnPlate = new List<GameObject>();
#endif


    private new void Awake()
    {
        base.Awake();

        deactivatedMaterial = meshRenderer.material;
        deactivatedWidth = transform.localScale.y;

        Refresh();
    }

    private void RemoveWeight(GameObject weight)
    {
        objectsOnPlate.Remove(weight);
        if (objectsOnPlate.Count == 0)
        {
            Deactivate();
        }
    }

    private void AddWeight(GameObject weight)
    {
        objectsOnPlate.Add(weight);
        if (currentState != SwitchState.Active)
        {
            Activate();
        }
    }   

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled)
        {
            return;
        }

        if (playerExclusive)
        {
            if (!other.gameObject.CompareTag("Player"))
            {
                Debug.LogWarning("Ignoring object '" + other.gameObject.name + "' on plate switch because it is not tagged as 'Player'.");
                return;
            }
        }

        AddWeight(other.gameObject);
    }

    /// <remarks>
    /// Respawned elements (such as cubes, rocks, and masks) may not trigger this event when they are destroyed or removed from the scene,
    /// potentially causing the plate to remain activated.
    /// </remarks>
    private void OnTriggerExit(Collider other)
    {
        if (!enabled)
        {
            return;
        }

        RemoveWeight(other.gameObject);
    }

    private void Update()
    {
        // Remove objects that have been destroyed or teleported away
        objectsOnPlate.RemoveWhere(obj => obj == null || !IsObjectStillOnPlate(obj));

        // Detect new objects that have spawned or teleported inside the trigger but were not registered
        Collider plateCollider = GetComponent<Collider>();
        if (plateCollider != null)
        {
            Collider[] overlapping = Physics.OverlapBox(
                plateCollider.bounds.center,
                plateCollider.bounds.extents,
                plateCollider.transform.rotation,
                includedLayersInCheck, // all layers
                QueryTriggerInteraction.Collide
            );

            foreach (var col in overlapping)
            {
                GameObject obj = col.gameObject;

                if (obj == null || obj == gameObject)
                {
                    continue;
                }

                if (playerExclusive && !obj.CompareTag("Player"))
                {
                    continue;
                }

                if (!objectsOnPlate.Contains(obj))
                {
                    AddWeight(obj);
                }
            }
        }

#if UNITY_EDITOR
        debugObjectsOnPlate.Clear();
        debugObjectsOnPlate.AddRange(objectsOnPlate);
#endif

        if (objectsOnPlate.Count == 0 && currentState == SwitchState.Active)
        {
            Debug.Log("Deactivating plate switch due to no objects detected on plate.");
            Deactivate();
        }
    }

    /// <summary>
    /// Determines whether the specified GameObject is still physically overlapping this plate's collider.
    /// Returns false if the object or its collider is missing, or if the bounds do not intersect.
    /// Used to detect objects that have been teleported, destroyed, or otherwise removed without triggering OnTriggerExit.
    /// </summary>
    /// <param name="obj">The GameObject to check.</param>
    /// <returns>True if the object's collider is still overlapping the plate's collider; otherwise, false.</returns>
    private bool IsObjectStillOnPlate(GameObject obj)
    {
        if (obj == null)
        {
            return false;
        }

        Collider col = obj.GetComponent<Collider>();

        if (col == null)
        {
            return false;
        }

        // Check if the collider is still overlapping this plate's collider
        Collider plateCollider = GetComponent<Collider>();

        if (plateCollider == null)
        {
            return false;
        }

        // Use bounds check as a simple overlap test
        return plateCollider.bounds.Intersects(col.bounds);
    }

    protected override void SetActive()
    {
        meshRenderer.material = activatedMaterial;
        transform.localScale = new Vector3(transform.localScale.x, activatedWidth, transform.localScale.z);
    }

    protected override void SetInactive()
    {
        meshRenderer.material = deactivatedMaterial;
        transform.localScale = new Vector3(transform.localScale.x, deactivatedWidth, transform.localScale.z);
    }

    protected override void SetOvertime()
    {
        throw new System.NotImplementedException();
    }
}

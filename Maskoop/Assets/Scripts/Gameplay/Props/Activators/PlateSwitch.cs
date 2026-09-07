using System.Collections.Generic;
using UnityEngine;

public class PlateSwitch : BaseSwitch
{
    [Header("Activation Settings")]
    [Tooltip("Whether the pressure plate can only be activated by the player.")]
    [SerializeField] protected bool isPlayerExclusive = true;
    [Tooltip("Layer mask to specify which layers should be considered when checking for objects on the plate.")]
    [SerializeField] protected LayerMask includedLayersInCheck = ~0;

    [Header("Apparience Settings")]
    [SerializeField] protected Material activatedMaterial;
    [SerializeField] protected float activatedWidth = 0.1f;

    protected Material deactivatedMaterial;
    protected float deactivatedWidth;

    private readonly HashSet<GameObject> objectsOnPlate = new HashSet<GameObject>();

#if UNITY_EDITOR
    [Header("Debug Settings")]
    [SerializeField] private List<GameObject> debugObjectsOnPlate = new List<GameObject>();
#endif

    private new void Awake()
    {
        base.Awake();
        deactivatedMaterial = meshRenderer.material;
        deactivatedWidth = transform.localScale.y;
        Refresh();
    }

    private bool IsValidObject(GameObject obj)
    {
        if (obj == null) return false;
        if (((1 << obj.layer) & includedLayersInCheck) == 0) return false;
        if (isPlayerExclusive && !obj.CompareTag("Player")) return false;
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidObject(other.gameObject)) return;

        objectsOnPlate.Add(other.gameObject);
        UpdateDebugList();

        if (currentState != SwitchState.Active)
        {
            Debug.Log("Activating plate switch due to objects detected on plate.");
            AudioSystem.PlaySFX(AudioSystem.SoundLibrary.pressurePlateOn, transform.position);
            Activate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!objectsOnPlate.Remove(other.gameObject)) return;
        UpdateDebugList();

        if (objectsOnPlate.Count == 0 && currentState == SwitchState.Active)
        {
            Debug.Log("Deactivating plate switch due to no objects detected on plate.");
            AudioSystem.PlaySFX(AudioSystem.SoundLibrary.pressurePlateOff, transform.position);
            Deactivate();
        }
    }

    private void UpdateDebugList()
    {
#if UNITY_EDITOR
        debugObjectsOnPlate.Clear();
        debugObjectsOnPlate.AddRange(objectsOnPlate);
#endif
    }

    protected override void SetActive()
    {
        meshRenderer.material = activatedMaterial;
    }

    protected override void SetInactive()
    {
        meshRenderer.material = deactivatedMaterial;
    }

    protected override void SetOvertime()
    {
        throw new System.NotImplementedException();
    }
}
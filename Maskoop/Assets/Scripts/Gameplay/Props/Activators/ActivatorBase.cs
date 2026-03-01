using UnityEngine;

public abstract class ActivatorBase : MonoBehaviour
{
    [Header("Connection Settings")]
    [Tooltip("Channel to use to connect to activables. Activables need to have the same channel be send a message")]
    [SerializeField] protected int channel = 1;

    [Header("Apparience Settings")]
    [SerializeField] protected Material activatedMaterial;
    [SerializeField] protected float activatedWidth = 0.1f;

    protected bool hasBeenActivated = false;

    protected MeshRenderer meshRenderer;
    protected Material deactivatedMaterial;
    protected float deactivatedWidth;


    protected abstract void SetApparience();

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        deactivatedMaterial = meshRenderer.material;
        deactivatedWidth = transform.localScale.z;

        SetApparience();
    }
}

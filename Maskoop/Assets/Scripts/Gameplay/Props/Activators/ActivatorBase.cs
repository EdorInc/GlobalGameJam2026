using UnityEngine;

public abstract class ActivatorBase : MonoBehaviour
{
    [Header("Connection Settings")]
    [Tooltip("Channel to use to connect to activables. Activables need to have the same channel be send a message")]
    [SerializeField] protected int channel = 1;

    [Header("Apparience Settings")]
    [SerializeField] protected Material activatedMaterial;

    protected bool hasBeenActivated = false;

    protected MeshRenderer meshRenderer;
    protected Material deactivatedMaterial;


    protected abstract void SetApparience();

    protected abstract void GetApparience();

    protected void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        GetApparience();
        SetApparience();
    }
}

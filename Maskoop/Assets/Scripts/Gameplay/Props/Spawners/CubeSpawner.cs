using UnityEngine;
using static BaseActivable;

public class CubeSpawner : BaseSpawner
{
    [Header("Conection Settings")]
    [SerializeField]
    [Tooltip("Channel to use to connect to buttons. Buttons need to have the same channel to activate this object.")]
    protected int channel = 1;

    private void OnEnable()
    {
        EventManager.OnButtonPressed += OnActivatorRecived;
    }

    private void OnDisable()
    {
        EventManager.OnButtonPressed -= OnActivatorRecived;
    }

    public void OnActivatorRecived(int channel)
    {
        if (this.channel == channel)
        {
            DestroyAndRespawnObject();
        }
    }
}

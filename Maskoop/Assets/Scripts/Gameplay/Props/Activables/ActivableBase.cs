using UnityEngine;

public abstract class ActivableBase : MonoBehaviour
{
    [Header("Conection Settings")]
    [SerializeField]
    [Tooltip("Channel to use to connect to buttons. Buttons need to have the same channel to activate this object")]
    protected int channel = 1;
    [SerializeField]
    [Tooltip("Amount of activators(buttons,targets,pressure plates) needed to activate this object")]
    protected int activatorsNeeded = 1;

    protected int activatorsLeft = 1;

    void Start()
    {
        //Link the function to the unity action emmited by targets and buttons
        EventManager.OnButtonPressed += OnActivatorRecived;
        activatorsLeft = activatorsNeeded;
    }

    public abstract void OnActivatorRecived(int channel);
}

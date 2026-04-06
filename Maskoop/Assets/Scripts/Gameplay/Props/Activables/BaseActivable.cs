using UnityEngine;

public abstract class BaseActivable : MonoBehaviour
{
    public enum ActivableState
    {
        Inactive,
        Activating,
        Active,
        Deactivating,
        Paused
    }

    [Header("Conection Settings")]
    [SerializeField]
    [Tooltip("Channel to use to connect to buttons. Buttons need to have the same channel to activate this object")]
    protected int channel = 1;
    [SerializeField]
    [Tooltip("Whether the activable will remain active")]
    protected bool keepActivated = false;
    [SerializeField]
    [Tooltip("Amount of activators needed to activate this object")]
    protected int activatorsNeeded = 1;

    protected int activatorsLeft = 1;

    protected ActivableState currentState = ActivableState.Inactive;


    protected void Start()
    {
        activatorsLeft = activatorsNeeded;
    }

    private void OnEnable()
    {
        EventManager.OnButtonPressed += OnActivatorRecived;
        EventManager.OnButtonUnPressed += OnActivatorRemoved;
    }

    private void OnDisable()
    {
        EventManager.OnButtonPressed -= OnActivatorRecived;
        EventManager.OnButtonUnPressed -= OnActivatorRemoved;
    }

    void Update()
    {
        switch (currentState)
        {
            case ActivableState.Activating:
                ActivateAnimation();
                if (StopCondition())
                {
                    currentState = ActivableState.Active;

                    // Call the function with the activation logic
                    OnActivation();
                }
                break;
            case ActivableState.Deactivating:
                DeactivateAnimation();
                if (StopCondition())
                {
                    currentState = ActivableState.Inactive;
                }
                break;
            case ActivableState.Paused:
                break;
        }
    }

    public void OnActivation()
    {
        if (keepActivated)
        {
            Debug.Log("Switches on channel " + channel + " are being locked...");
            EventManager.OnButtonLock?.Invoke(channel);
        }
    }

    public void OnActivatorRecived(int channel)
    {
        if (this.channel == channel)
        {
            activatorsNeeded--;
            Debug.Log("Activating channel " + channel + " on " + gameObject.name + ".");

            if (activatorsNeeded == 0)
            {
                currentState = ActivableState.Activating;
                // Debug.Log("All activators received for channel " + channel + " on object " + gameObject.name);
            }
        }
    }

    public void OnActivatorRemoved(int channel)
    {
        if (this.channel == channel)
        {
            activatorsNeeded++;

            if (activatorsNeeded > 0)
            {
                currentState = ActivableState.Deactivating;
                // Debug.Log("Deactivating channel " + channel + " on object " + gameObject.name);
            }
        }
    }

    /// <summary>
    /// Returns true when the activation or deactivation process is complete.
    /// The stopping logic should account for both activating and deactivating states, as the condition may differ.
    /// </summary>
    protected abstract bool StopCondition();

    protected abstract void ActivateAnimation();

    protected abstract void DeactivateAnimation();

}

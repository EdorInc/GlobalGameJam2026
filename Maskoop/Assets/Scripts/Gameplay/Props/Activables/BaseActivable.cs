using Unity.VisualScripting;
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
    public int channel = 1;
    [SerializeField]
    [Tooltip("Whether the activable will remain active after completing the animation")]
    protected bool keepActivatedOnCompletion = false;
    [SerializeField]
    [Tooltip("Whether the activable will remain active once all activator needed are pressed")]
    protected bool keepActivatedOnSimultaneousPress = false;
    [SerializeField]
    [Tooltip("Amount of activators needed to activate this object")]
    protected int activatorsNeeded = 1;

#if UNITY_EDITOR
    [SerializeField]
#endif
    protected int activatorsLeft = 1;

    protected ActivableState currentState = ActivableState.Inactive;

    protected bool areButtonsLocked = false;

    protected void Start()
    {
        activatorsLeft = activatorsNeeded;
        EventManager.PairDoor?.Invoke(this.transform, channel);
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

    protected void Lock()
    {
        if (!areButtonsLocked)
        {
            Debug.Log("Switches on channel " + channel + " are being locked...");
            EventManager.OnButtonLock?.Invoke(channel);
            areButtonsLocked = true;
        }
    }

    public void OnActivation()
    {
        if (keepActivatedOnCompletion)
        {
            Lock();
        }
    }

    public void OnActivatorRecived(int channel)
    {
        if (this.channel == channel)
        {
            ActivatorOn();

            Debug.Log("Activating channel " + channel + " on " + gameObject.name + ".");

            if (activatorsLeft == 0)
            {
                if(keepActivatedOnSimultaneousPress)
                {
                    Lock();
                }

                currentState = ActivableState.Activating;
                // Debug.Log("All activators received for channel " + channel + " on object " + gameObject.name);
            }
        }
    }

    public void OnActivatorRemoved(int channel)
    {
        if (this.channel == channel)
        {
            ActivatorOff();

            if (activatorsLeft > 0)
            {
                currentState = ActivableState.Deactivating;
                // Debug.Log("Deactivating channel " + channel + " on object " + gameObject.name);
            }
        }
    }

    private void ActivatorOn()
    {
        activatorsLeft--;

        // if (activatorsLeft < 0)
        // {
        //     activatorsLeft = 0;
        // }
    }

    private void ActivatorOff()
    {
        activatorsLeft++;

        if (activatorsLeft > activatorsNeeded)
        {
            activatorsLeft = activatorsNeeded;
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

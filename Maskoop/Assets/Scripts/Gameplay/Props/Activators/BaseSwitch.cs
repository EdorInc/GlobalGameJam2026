using UnityEngine;

public abstract class BaseSwitch : MonoBehaviour
{
    public enum SwitchState
    {
        Inactive,
        Active,
        Pending
    }

    [Header("Connection Settings")]
    [Tooltip("Channel to use to connect to activables. Activables need to have the same channel be send a message")]
    [SerializeField] public int channel = 1;

    [Tooltip("Whether the pressure plate remains activated after the player leaves.")]
    [SerializeField] protected bool keepActivated = false;

    protected SwitchState currentState = SwitchState.Inactive;

    protected MeshRenderer meshRenderer;

    protected void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    protected void OnEnable()
    {
        EventManager.OnButtonLock += OnLockReceived;
    }

    protected void OnDisable()
    {
        EventManager.OnButtonLock -= OnLockReceived;
    }

    protected virtual void OnLockReceived(int receivedChannel)
    {
        if (receivedChannel == channel)
        {
            currentState = SwitchState.Active;

            Refresh();

            // Disable this script permanently.
            enabled = false;
        }
    }

    /// <summary>
 	/// Activates the switch, sets its state to Active, refreshes its appearance, and invokes the OnButtonPressed event.
 	/// </summary>
    /// <remarks>
    /// If keepActivated is true this will disable the component after activating.
    /// </remarks>
    protected virtual void Activate()
    {
        currentState = SwitchState.Active;

        Refresh();

        EventManager.OnButtonPressed?.Invoke(channel);

        if (keepActivated)
        {
            // Disable this script permanently.
            enabled = false;
        }
    }

    /// <summary>
    /// Deactivates the switch, sets its state to Inactive, refreshes its appearance, and invokes the OnButtonUnPressed event.
    /// </summary>
    protected virtual void Deactivate()
    {
        currentState = SwitchState.Inactive;
    
        Refresh();

        EventManager.OnButtonUnPressed?.Invoke(channel);
    }

    /// <summary>
    /// Sets the switch state to Pending, refreshes its appearance, and optionally invokes the OnButtonPressed event if coming from Inactive.
    /// </summary>
    /// <remarks>
    /// If keepActivated is true Pending is treated as Active.
    /// </remarks>
    protected virtual void Overtime()
    {
        if (keepActivated)
        {
            Debug.LogWarning("Skipping to Active state...");

            // If keep activated skip the pending state.
            Activate();
        }
        else
        {
            if (currentState == SwitchState.Inactive)
            {
                EventManager.OnButtonPressed?.Invoke(channel);
            }

            currentState = SwitchState.Pending;

            Refresh();
        }
    }

    protected void Refresh()
    {
        if (!enabled)
        {
            return;
        }

        switch (currentState)
        {
            case SwitchState.Active:
                SetActive();
                break;
            case SwitchState.Inactive:
                SetInactive();
                break;
            case SwitchState.Pending:
                SetOvertime();
                break;
            default:
                Debug.LogError("Target is in an unknown state.");
                break;
        }
    }

    protected abstract void SetActive();

    protected abstract void SetInactive();

    protected abstract void SetOvertime();
}

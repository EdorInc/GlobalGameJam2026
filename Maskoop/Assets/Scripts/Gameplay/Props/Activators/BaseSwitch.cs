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
    [SerializeField] protected int channel = 1;

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

            //Debug.Log("The switch on " + gameObject.name + " has been locked.");

            // Disable this script permanently
            enabled = false;
        }
    }

    /// <summary>
 	/// Activates the switch, sets its state to Active, refreshes its appearance, and invokes the OnButtonPressed event.
 	/// </summary>
    protected virtual void Activate()
    {
        currentState = SwitchState.Active;

        Refresh();

        EventManager.OnButtonPressed?.Invoke(channel);
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
    protected virtual void Overtime()
    {
        if (currentState == SwitchState.Inactive)
        {
            EventManager.OnButtonPressed?.Invoke(channel);
        }

        currentState = SwitchState.Pending;

        Refresh();
    }

    protected void Refresh()
    {
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

using UnityEngine;

public abstract class BaseSwitch : MonoBehaviour
{
    public enum SwitchState
    {
        Inactive,
        Active,
        Deactivating
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

    protected virtual void Activate()
    {
        currentState = SwitchState.Active;

        Refresh();

        EventManager.OnButtonPressed?.Invoke(channel);
    }

    protected virtual void Deactivate()
    {
        currentState = SwitchState.Inactive;
    
        Refresh();

        EventManager.OnButtonUnPressed?.Invoke(channel);
    }

    protected virtual void Overtime()
    {
        currentState = SwitchState.Deactivating;

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
            case SwitchState.Deactivating:
                SetDeactivating();
                break;
            default:
                Debug.LogError("Target is in an unknown state.");
                break;
        }
    }

    protected abstract void SetActive();

    protected abstract void SetInactive();

    protected abstract void SetDeactivating();
}

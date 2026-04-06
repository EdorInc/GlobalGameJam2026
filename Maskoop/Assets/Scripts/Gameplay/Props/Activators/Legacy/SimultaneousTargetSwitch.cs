using UnityEngine;

public class SimultaneousTargetSwitch : TargetSwitch
{
    [Header("Deactivation Settings")]
    [Tooltip("Time the switch is active for.")]
    [SerializeField] protected bool alwaysDeactivate = false;
    [Tooltip("Time the switch is active for.")]
    [SerializeField] protected float activeTime = 1f;
    [Tooltip("Time the switch is blinking for before deactivating.")]
    [SerializeField] protected float blinkingTime = 1f;
    [Tooltip("Time between blinks.")]
    [SerializeField] private float blinkInterval = 0.2f;

    protected float blinkTimer = 0;
    protected bool isBlinkOn = false;
    protected float currentActiveTime = Mathf.Infinity;
    protected float currentBlinkingTime = Mathf.Infinity;

    private void Update()
    {
        if(currentState == SwitchState.Active)
        {
            currentActiveTime -= Time.deltaTime;
            if(currentActiveTime <= 0)
            {
                Overtime();
            }
        }
        else if(currentState == SwitchState.Pending)
        {
            HandleBlinking();
            currentBlinkingTime -= Time.deltaTime;
            if(currentBlinkingTime <= 0)
            {
                Deactivate();
            }
        }
    }
    protected override void SetActive()
    {
        base.SetActive();
        currentActiveTime = activeTime;
    }

    protected override void SetOvertime()
    {
        currentBlinkingTime = blinkingTime;
        blinkTimer = blinkInterval;
        isBlinkOn = false;
    }

    private void HandleBlinking()
    {
        blinkTimer -= Time.deltaTime;

        if (blinkTimer <= 0f)
        {
            blinkTimer = blinkInterval;
            isBlinkOn = !isBlinkOn;

            meshRenderer.material = isBlinkOn ? deactivatedMaterial : activatedMaterial;
        }
    }
}

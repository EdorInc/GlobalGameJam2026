using UnityEngine;
using System.Collections;

public class TargetSwitch : BaseSwitch
{
    [Header("Apparience Settings")]
    [Tooltip("Provisional material to use when the target is activated.")]
    [SerializeField] protected Material activatedMaterial;

    [Tooltip("Provisional material to use when the target is pending.")]
    [SerializeField] protected Material pendingMaterial;

    [Tooltip("Provisinal width to use when the target is activated.")]
    [SerializeField] protected float activatedWidth = 0.1f;

    [Header("Timer Settings")]
    [Tooltip("Timer for switch to deactivate")]
    [SerializeField] protected float activatedTimer = 2.0f;

    [Tooltip("Timer for switch to deactivate after overtime")]
    [SerializeField] protected float overtimeTimer = 1.0f;

    [Header("Blink Settings")]
    [Tooltip("Time between blinks when the target is pending.")]
    [SerializeField] protected float blinkIntervals = 0.2f;

    protected float blinkElapsed = 0f;

    protected bool isBlinking = false;
    protected bool blinkingState = false;

    protected Material deactivatedMaterial;
    protected float deactivatedWidth;

    private Coroutine deactivateCoroutine;

    private void OnValidate()
    {
        // Ensure overtimeTimer is always less than activatedTimer
        if (overtimeTimer >= activatedTimer)
        {
            overtimeTimer = Mathf.Max(0f, activatedTimer - 0.01f);
        }
    }

    private new void Awake()
    {
        base.Awake();

        deactivatedMaterial = meshRenderer.material;
        deactivatedWidth = transform.localScale.z;

        Refresh();
    }

    protected new void OnDisable()
    {
        base.OnDisable();

        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!enabled)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Rock") && currentState != SwitchState.Active)
        {
            Overtime();
        }
    }

    protected override void Overtime()
    {
        base.Overtime();

        if(!enabled)
        {
            return;
        }

        // Reset the timer if already running
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
        }

        deactivateCoroutine = StartCoroutine(DeactivateAfterDelay());
    }

    /// <summary>
    /// Blinks the target using the overtime material until deactivation.
    /// </summary>
    private IEnumerator Blink()
    {
        float blinkDuration = overtimeTimer;
        float startTime = Time.time;

        while (Time.time - startTime < blinkDuration && currentState == SwitchState.Pending)
        {
            blinkingState = !blinkingState;
            meshRenderer.material = blinkingState ? pendingMaterial : deactivatedMaterial;
            yield return new WaitForSeconds(blinkIntervals);
        }

        // Ensure the material is set to inactive just before deactivation
        meshRenderer.material = deactivatedMaterial;
    }

    private IEnumerator DeactivateAfterDelay()
    {
        float elapsed = 0f;
        bool isBlinking = false;

        // Wait until overtimeTimer is reached, then start blinking
        while (elapsed < activatedTimer)
        {
            if (!isBlinking && elapsed >= activatedTimer - overtimeTimer)
            {
                isBlinking = true;
                StartCoroutine(Blink());
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Deactivate();
        deactivateCoroutine = null;
    }

    protected override void SetActive()
    {
        meshRenderer.material = activatedMaterial;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, activatedWidth);
    }

    protected override void SetInactive()
    {
        meshRenderer.material = deactivatedMaterial;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, deactivatedWidth);
    }

    protected override void SetOvertime()
    {
        meshRenderer.material = pendingMaterial;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, activatedWidth);
    }
}

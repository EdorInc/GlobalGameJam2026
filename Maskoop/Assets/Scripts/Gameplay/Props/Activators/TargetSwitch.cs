using UnityEngine;
using System.Collections;

public class TargetSwitch : BaseSwitch
{
    [Header("Apparience Settings")]
    [Tooltip("Provisinal material to use when the target is activated.")]
    [SerializeField] protected Material activatedMaterial;

    [Tooltip("Provisinal material to use when the target is pending.")]
    [SerializeField] protected Material pendingMaterial;

    [Tooltip("Provisinal width to use when the target is activated.")]
    [SerializeField] protected float activatedWidth = 0.1f;

    [Header("Timer Settings")]
    [Tooltip("Timer for switch to deactivate")]
    [SerializeField] protected float activatedTimer = 2.0f;

    protected Material deactivatedMaterial;
    protected float deactivatedWidth;

    private Coroutine deactivateCoroutine;

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

        // Debug.Log("Shutting down the switch on " + gameObject.name + " and stopping any active timers.");

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

    // private void SwitchTimer()
    // {
    //     if (resetNeeded)
    //     {
    //         timeActivated = 0;
    //         resetNeeded = false;
    //     }
    // 
    //     if (currentState == SwitchState.Active)
    //     {
    //         timeActivated += Time.deltaTime;
    //         Debug.Log("Time passed: " + timeActivated + " Time it needs: " + activatedTimer);
    //         if (timeActivated > activatedTimer)
    //         {
    //             Deactivate();
    //             resetNeeded = true;
    //         }
    //     }
    // }

    protected override void Overtime()
    {
        base.Overtime();

        // Reset the timer if already running
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
        }
        deactivateCoroutine = StartCoroutine(DeactivateAfterDelay());
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(activatedTimer);
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

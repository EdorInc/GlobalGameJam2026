using UnityEngine;

public class TargetSwitch : BaseSwitch
{
    [Header("Apparience Settings")]
    [Tooltip("Provisinal material to use when the target is activated.")]
    [SerializeField] protected Material activatedMaterial;

    [Tooltip("Provisinal width to use when the target is activated.")]
    [SerializeField] protected float activatedWidth = 0.1f;

    [Header("Timer Settings")]
    [Tooltip("Timer for switch to deactivate")]
    [SerializeField] protected float activatedTimer = 2.0f;

    protected float timeActivated = 0;
    protected bool resetNeeded = true;

    protected Material deactivatedMaterial;

    protected float deactivatedWidth;

    protected bool blinkActive = true;

    private new void Awake()
    {
        base.Awake();

        deactivatedMaterial = meshRenderer.material;
        deactivatedWidth = transform.localScale.z;

        Refresh();
    }

    private void Update()
    {
        SwitchTimer();
    } 

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rock") && currentState != SwitchState.Active)
        {
            Activate();
        }
    }

    private void SwitchTimer()
    {
        if (resetNeeded)
        {
            timeActivated = 0;
            resetNeeded = false;
        }

        if (currentState == SwitchState.Active)
        {
            timeActivated += Time.deltaTime;
            Debug.Log("Time passed: " + timeActivated + " Time it needs: " + activatedTimer);
            if (timeActivated > activatedTimer)
            {
                Deactivate();
                resetNeeded = true;
            }
        }
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

    protected override void SetDeactivating()
    {
        throw new System.NotImplementedException();
    }
}

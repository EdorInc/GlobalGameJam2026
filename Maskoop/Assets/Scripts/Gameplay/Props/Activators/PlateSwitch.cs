using UnityEngine;

public class PlateSwitch : BaseSwitch
{
    [Header("Activation Settings")]
    [Tooltip("Whether the pressure plate can only be activated by the player.")]
    [SerializeField] protected bool playerExclusive = true;

    [Header("Apparience Settings")]
    [Tooltip("Provisinal material to use when the target is activated.")]
    [SerializeField] protected Material activatedMaterial;
    [Tooltip("Provisinal width to use when the target is activated.")]
    [SerializeField] protected float activatedWidth = 0.1f;

    protected Material deactivatedMaterial;

    protected float deactivatedWidth;

    protected int weightsOnPlate = 0;

    private new void Awake()
    {
        base.Awake();

        deactivatedMaterial = meshRenderer.material;
        deactivatedWidth = transform.localScale.y;

        Refresh();
    }

    private void OnTriggerEnter(Collider other)
    {
        bool willActivate = false;

        if (playerExclusive)
        {
            bool isPlayer = other.gameObject.CompareTag("Player");

            if (isPlayer)
            {
                willActivate = currentState != SwitchState.Active;

                Debug.Log("Player stepped on the plate, added a weight.");  
                weightsOnPlate++;
            }
        }
        else
        {
            willActivate = currentState != SwitchState.Active;

            Debug.Log("An object stepped on the plate, added a weight.");
            weightsOnPlate++;
        }

        if (willActivate)
        {
            Debug.Log("The plate switch is now active.");
            Activate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerExclusive)
        {
            bool isPlayer = other.gameObject.CompareTag("Player");

            if (isPlayer)
            {
                weightsOnPlate--;
            }
        } 
        else
        {
            weightsOnPlate--;
        }

        if (weightsOnPlate <= 0)
        {
            // To avoid possible bugs with multiple objects on the plate, we reset the counter to 0 when it goes negative.
            weightsOnPlate = 0;

            Debug.Log("The last weight left the plate, deactivating...");
            Deactivate();
        }
    }

    protected override void SetActive()
    {
        meshRenderer.material = activatedMaterial;
        transform.localScale = new Vector3(transform.localScale.x, activatedWidth, transform.localScale.z);
    }

    protected override void SetInactive()
    {
        meshRenderer.material = deactivatedMaterial;
        transform.localScale = new Vector3(transform.localScale.x, deactivatedWidth, transform.localScale.z);
    }

    protected override void SetDeactivating()
    {
        throw new System.NotImplementedException();
    }
}

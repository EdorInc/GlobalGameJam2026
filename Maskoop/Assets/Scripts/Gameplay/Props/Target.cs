using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Connection Settings")]
    [Tooltip("Channel to use to connect to doors. Doors need to have the same channel be send a message")]
    [SerializeField] private int channel = 1;

    [Header("Apparience Settings")]
    [SerializeField] Material activatedMaterial;
    [SerializeField] float activatedWidth = 0.1f;

    private bool hasBeenActivated = false;

    private MeshRenderer meshRenderer;
    private Material deactivatedMaterial;
    private float deactivatedWidth;

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        deactivatedMaterial = meshRenderer.material;
        deactivatedWidth = transform.localScale.z;

        SetApparience();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rock") && !hasBeenActivated)
        {
            hasBeenActivated = true;

            SetApparience();

            EventManager.OnButtonPressed?.Invoke(channel);
        }
    }

    void SetApparience()
    {
        if (hasBeenActivated)
        {
            meshRenderer.material = activatedMaterial;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, activatedWidth);
        }
        else
        {
            meshRenderer.material = deactivatedMaterial;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, deactivatedWidth);
        }
    }
}

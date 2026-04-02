using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Grabbable : GroundDetector
{
    [Header("Grab Settings")]
    [Tooltip("Local position offset when the object is held.")]
    public Vector3 holdOffset = Vector3.zero;
    [Tooltip("Local rotation when the object is held.")]
    public Quaternion holdRotation = Quaternion.identity;

    [Header("Airborn Settings")]
    [Tooltip("If true, resets the object's rotation to its original value when in the air.")]
    public bool resetRotationInAir = true;

    [Header("Throw Settings")]
    [Tooltip("Maximum throw force (x: forward, y: up) that can be applied to this object.")]
    public Vector2 maxThrowForce = new Vector2(8, 8);
    [Tooltip("Minimum throw force (x: forward, y: up) that can be applied to this object.")]
    public Vector2 minThrowForce = new Vector2(0, 0);
    [Tooltip("Rate at which the throw force increases while charging.")]
    public float forceGrowRate = 2;

    [Header("Highlight Settings")]
    [Tooltip("Layer to use when the object is highlighted.")]
    [SerializeField] private int highlightLayer = 9;

    [Tooltip("If true, the object will be highlighted when the player looks at it.")]
    [SerializeField] private bool highlightOnLook = true;

    private bool IsHighlighted = false;

    [HideInInspector]
    public bool IsGrabbed { get; set; }

    private int originalLayer;

    private Rigidbody rigidBody;

    private Quaternion originalRotation;

    protected override void Start()
    {
        base.Start();

        rigidBody = GetComponent<Rigidbody>();

        originalRotation = transform.rotation;
        originalLayer = gameObject.layer;
    }

    public void Highlight(bool highlight)
    {
        IsHighlighted = highlight;

        if (highlightOnLook) 
        {
            if (IsHighlighted)
            {
                SetLayerRecursively(gameObject, highlightLayer);
            }
            else
            {
                SetLayerRecursively(gameObject, originalLayer);
            }
        }
    }

    void Update()
    {
        if (IsGrounded)
        {
            rigidBody.isKinematic = true;
        } else
        {
            rigidBody.isKinematic = false;
            ResetFrozenRotation();
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    /// <summary>
    /// Resets the object's rotation for any axes that are frozen in Rigidbody constraints
    /// to their original values, but only when <see cref="resetRotationInAir"/> is true.
    /// This helps maintain consistent orientation for grabbable objects while airborne.
    /// </summary>
    void ResetFrozenRotation()
    {
        if (resetRotationInAir)
        {
            Vector3 currentEuler = transform.rotation.eulerAngles;
            Vector3 originalEuler = originalRotation.eulerAngles;

            RigidbodyConstraints constraints = rigidBody.constraints;

            // Check each frozen axis and restore only those
            if ((constraints & RigidbodyConstraints.FreezeRotationX) != 0)
                currentEuler.x = originalEuler.x;

            if ((constraints & RigidbodyConstraints.FreezeRotationY) != 0)
                currentEuler.y = originalEuler.y;

            if ((constraints & RigidbodyConstraints.FreezeRotationZ) != 0)
                currentEuler.z = originalEuler.z;

            transform.rotation = Quaternion.Euler(currentEuler);
        }
    }
}
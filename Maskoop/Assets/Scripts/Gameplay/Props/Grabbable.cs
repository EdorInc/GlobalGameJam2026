using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Grabbable : GroundDetector
{
    [Header("Grab Settings")]
    public Vector3 holdOffset = Vector3.zero;   // local position when held
    public Quaternion holdRotation = Quaternion.identity; // local rotation when held

    [Header("Airborn Settings")]
    public bool resetRotationInAir = true; // whether to reset rotation when in air
    public Quaternion originalRotation; // to store the original rotation for resetting

    [Header("Throw Settings")]
    public Vector2 maxThrowForce = new Vector2(8,8);
    public Vector2 minThrowForce = new Vector2(0, 0);
    public float forceGrowRate = 2;

    private Rigidbody rigidBody;

    protected override void Start()
    {
        base.Start();

        rigidBody = GetComponent<Rigidbody>();

        originalRotation = transform.rotation;
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

    void ResetFrozenRotation()
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
using UnityEngine;
using UnityEngine.InputSystem;

public class Throw : MonoBehaviour
{
    private Grab grabComponent;

    //[HideInInspector]
    public GameObject grabbedObject;

    [Header("Throw Settings")]
    [SerializeField]
    private float minForceFowards = 0;
    [SerializeField]
    private float minForceUp = 0;
    [SerializeField]
    private float maxForceFoward = 8;
    [SerializeField]
    private float maxForceUp = 8;
    [SerializeField]
    private float forceGrowRate = 2;

    [Header("Trajectory Settings")]
    [SerializeField]
    private float maxTime = 5.0f;
    [SerializeField]
    private float timeStep = 0.1f;
    [SerializeField]
    private float lineWidth = 0.5f;

    [Header("Vibration Settings")]
    [SerializeField]
    float lowVibrationIntensity = 0.1f;
    [SerializeField]
    float highVibrationIntensity = 0.1f;
    [SerializeField]
    float vibrationDuration = 0.01f;

    private LineRenderer trajectoryRenderer;

    private float currentForceFoward;
    private float currentForceUp;
    private bool charging = false;

    void Start()
    {
        trajectoryRenderer = GetComponent<LineRenderer>();

        if (trajectoryRenderer == null)
        {
            Debug.LogError("LineRenderer component missing from Throw script on " + gameObject.name);
            this.enabled = false;
        }

        grabComponent = GetComponent<Grab>();
        if (grabComponent == null)
        {
            Debug.LogError("Grab component missing from Throw script on " + gameObject.name);
            this.enabled = false;
        }
    }

    void Update()
    {
        if (grabComponent.grabbedObject != null) grabbedObject = grabComponent.grabbedObject;
        else grabbedObject = null;

        if (charging && grabbedObject != null)
        {
            Vibrate(lowVibrationIntensity, highVibrationIntensity, vibrationDuration);
            ChargeUpdate();
        }
    }

    private void Vibrate(float low, float high, float duration)
    {
        // if (this.gameObject == ReferenceManager.Instance.GetPlayerOne())
        // {
        //     Gamepad gamepad = ReferenceManager.Instance.GetPlayerOne().GetComponent<RegisterController>().GetPlayerGamepad();
        //     if (!gamepad.IsUnityNull())
        //     {
        //         UnityEngine.Debug.Log("Vibrating Player One's controller");
        //         VibrationManager.Instance.RumblePulse(gamepad, low, high, duration);
        //     }
        // 
        // }
        // else
        // {
        //     Gamepad gamepad = ReferenceManager.Instance.GetPlayerTwo().GetComponent<RegisterController>().GetPlayerGamepad();
        //     if (!gamepad.IsUnityNull())
        //     {
        //         UnityEngine.Debug.Log("Vibrating Player Two's controller");
        //         VibrationManager.Instance.RumblePulse(gamepad, low, high, duration);
        //     }
        // }
    }

    public void ChargeObject()
    {
        if (!charging && grabbedObject != null)
        {
            charging = true;
            currentForceUp = minForceUp;
            currentForceFoward = minForceFowards;
        }
        else if (!charging && grabbedObject == null)
        {
            Debug.Log("No object to throw.");
        }
    }

    private void ChargeUpdate()
    {
        if (charging && grabbedObject != null)
        {
            currentForceFoward += forceGrowRate * Time.deltaTime;
            currentForceUp += forceGrowRate * Time.deltaTime;
            currentForceFoward = Mathf.Min(currentForceFoward, maxForceFoward);
            currentForceUp = Mathf.Min(currentForceUp, maxForceUp);
        }

        DrawTrajectory(currentForceFoward, currentForceUp);
    }

    public void ThrowObject()
    {
        if (!grabbedObject || !grabComponent.grabbedObject)
        {
            Debug.Log("No object to throw.");
            return;
        }

        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();

        if (rb == null)
        {
            return;
        }
        else
        {
            Debug.Log("Thrown object " + grabbedObject.name);

            charging = false;
            grabbedObject = null;

            grabComponent.DropObject();

            rb.AddForce(transform.forward * currentForceFoward + Vector3.up * currentForceUp, ForceMode.Impulse);
        }

        ClearTrajectory();
    }

    public void DrawTrajectory(float impulseStrengthFoward, float impulseStrengthUp)
    {
        if(!trajectoryRenderer.enabled || !grabbedObject)
            Debug.Log("Something is missing, not drawing trajectory.");

        Vector3 initialPos = grabbedObject.transform.localPosition;
        float initialMass = grabbedObject.GetComponent<Rigidbody>().mass;

        Debug.Log($"Initial Position: {initialPos}");
        Debug.Log("Preview forward: " + transform.forward);

        Vector3 forwardVelocity = transform.forward * (impulseStrengthFoward / initialMass);
        Vector3 upwardVelocity = Vector3.up * (impulseStrengthUp / initialMass);
        Vector3 initialVelocity = forwardVelocity + upwardVelocity;

        trajectoryRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trajectoryRenderer.receiveShadows = false;

        trajectoryRenderer.startWidth = lineWidth;
        trajectoryRenderer.endWidth = lineWidth;

        trajectoryRenderer.material.mainTextureScale = new Vector2(1f / lineWidth, 1.0f);

        Vector3 previousPoint = initialPos;
        int pointsDrawn = 0;
        int maxSteps = Mathf.CeilToInt(maxTime / timeStep);

        bool hitFound = false;

        trajectoryRenderer.positionCount = maxSteps;

        for (int i = 0; i < maxSteps; i++)
        {
            float t = i * timeStep;

            Vector3 currentPoint = initialPos + initialVelocity * t + 0.5f * Physics.gravity * t * t;

            // Raycast between last point and this point
            Vector3 segment = currentPoint - previousPoint;
            float distance = segment.magnitude;

            if (Physics.Raycast(previousPoint, segment.normalized, out RaycastHit hit, distance))
            {
                // Stop at ground hit
                trajectoryRenderer.SetPosition(pointsDrawn, hit.point);
                
                // PlaceLandingMarker(hit);

                pointsDrawn++;
                hitFound = true;

                break;
            }

            trajectoryRenderer.SetPosition(pointsDrawn, currentPoint);

            previousPoint = currentPoint;
            pointsDrawn++;
        }

        // trajectoryRenderer.positionCount = pointsDrawn - 1;
        trajectoryRenderer.positionCount = pointsDrawn;

        // if (!hitFound && landingMarker != null)
        // {
        //     landingMarker.SetActive(false);
        // }
    }

    public void ClearTrajectory()
    {
        Debug.Log("Clearing trajectory");

        trajectoryRenderer.positionCount = 0;
        //landingMarker.SetActive(false);
    }
}

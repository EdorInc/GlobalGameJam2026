using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Throw : MonoBehaviour
{
    private Grab grabComponent;
    private Equip equipComponent;
    private LineRenderer trajectoryRenderer;
    
    [HideInInspector]
    public GameObject grabbedObject;


    void Start()
    {
        StartSimulation();

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

        equipComponent = GetComponent<Equip>();

        if (equipComponent == null)
        {
            Debug.LogError("Equip component missing from Throw script on " + gameObject.name);
            this.enabled = false;
        }
    }

    private void StartSimulation()
    {
        if (!simulationObstaclesParent)
        {
            Debug.LogWarning("Simulation Obstacles Parent is not assigned. Changing to default trajectory calculation...");
            useSimulation = false;
            return;
        }

        Scene simulationScene = SceneManager.GetSceneByName("Simulation");

        if (!simulationScene.IsValid())
        {
            simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));

            physicsScene = simulationScene.GetPhysicsScene();

            foreach (Transform obj in simulationObstaclesParent)
            {
                var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);
                Renderer[] ghostRenderers = ghostObj.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in ghostRenderers) r.enabled = false;
                SceneManager.MoveGameObjectToScene(ghostObj, simulationScene);
                if (!ghostObj.isStatic) spawnedObjects.Add(obj, ghostObj.transform);
            }
        }
        else
        {
            physicsScene = simulationScene.GetPhysicsScene();
        }
    }

    [Header("Vibration Settings")]
    [SerializeField]
    float lowVibrationIntensity = 0.1f;
    [SerializeField]
    float highVibrationIntensity = 0.1f;
    [SerializeField]
    float vibrationDuration = 0.01f;

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

    private float minForceFowards = 0;
    private float minForceUp = 0;
    private float maxForceFoward = 8;
    private float maxForceUp = 8;
    private float forceGrowRate = 2;

    private float currentForceFoward;
    private float currentForceUp;
    public bool charging = false;

    public void ChargeObject()
    {
        if (equipComponent.IsMaskEquiped())
        {
            equipComponent.ChangeEquipState();
            grabbedObject = grabComponent.grabbedObject;
        }
        if (!charging && grabbedObject != null)
        {

            minForceFowards = grabbedObject.GetComponent<Grabbable>().minThrowForce.x;
            minForceUp = grabbedObject.GetComponent<Grabbable>().minThrowForce.y;
            maxForceFoward = grabbedObject.GetComponent<Grabbable>().maxThrowForce.x;
            maxForceUp = grabbedObject.GetComponent<Grabbable>().maxThrowForce.y;
            forceGrowRate = grabbedObject.GetComponent<Grabbable>().forceGrowRate;

            charging = true;
            currentForceUp = minForceUp;
            currentForceFoward = minForceFowards;
        }
        else if (!charging && grabbedObject == null)
        {
            EventManager.OnCantPerforAction?.Invoke(gameObject);
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
            EventManager.Throw?.Invoke(grabbedObject,true,gameObject);
            charging = false;
            grabbedObject = null;

            grabComponent.DropObject();

            rb.AddForce(transform.forward * currentForceFoward + Vector3.up * currentForceUp, ForceMode.Impulse);
            
        }

        ClearTrajectory();
    }

    [Header("Trajectory Settings")]
    [SerializeField]
    private float maxTime = 5.0f;
    [SerializeField]
    private int endPointsIgnored = 1;
    [SerializeField]
    private GameObject landingMarker;
    [SerializeField]
    private bool useSimulation = false;
    [SerializeField] 
    private Transform simulationObstaclesParent;

    private Scene simulationScene;
    private PhysicsScene physicsScene;
    private readonly Dictionary<Transform, Transform> spawnedObjects = new Dictionary<Transform, Transform>();
    
    public void DrawTrajectory(float impulseStrengthFoward, float impulseStrengthUp)
    {
        if (!trajectoryRenderer.enabled || !grabbedObject)
            return;

        Rigidbody objectRigidbody = grabbedObject.GetComponent<Rigidbody>();
        Collider objectCollider = grabbedObject.GetComponent<Collider>();

        if (objectRigidbody == null)
        {
            Debug.LogError("Grabbed object must have a Rigidbody component.");
            return;
        }

        float objectMass = objectRigidbody.mass;

        Vector3 forwardVelocity = transform.forward * (impulseStrengthFoward / objectMass);
        Vector3 upwardVelocity = Vector3.up * (impulseStrengthUp / objectMass);

        Vector3 launchPosition = grabbedObject.transform.position;
        Vector3 launchVelocity = forwardVelocity + upwardVelocity;
        Vector3 launchForce = transform.forward * impulseStrengthFoward + Vector3.up * impulseStrengthUp;

        if (useSimulation)
        {
            SimulateTrajectory(grabbedObject, launchForce);
        }
        else
        {
            CalculateTrajectory(launchPosition, launchVelocity);
        }
    }

    private void SimulateTrajectory(GameObject gameObject, Vector3 velocity)
    {
        if(gameObject == null)
        {
            Debug.LogError("No object to simulate trajectory for.");
            return;
        }

        GameObject ghostObj = Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation);
        Renderer[] ghostRenderers = ghostObj.GetComponentsInChildren<Renderer>();
        Collider ghostCollider = ghostObj.GetComponent<Collider>();
        Rigidbody ghostRigidbody = ghostObj.GetComponent<Rigidbody>();

        SceneManager.MoveGameObjectToScene(ghostObj, simulationScene);

        foreach (Renderer r in ghostRenderers)  r.enabled = false;
        if (ghostCollider) ghostCollider.enabled = true;
        if (ghostRigidbody)
        {
            ghostRigidbody.isKinematic = false;
            ghostRigidbody.useGravity = true;
            ghostRigidbody.AddForce(velocity, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Object " + gameObject.name + " has no Rigidbody, cannot simulate trajectory.");
            return;
        }

        bool ghostStopped = false;
        int maxSteps = Mathf.CeilToInt(maxTime / Time.fixedDeltaTime);
        trajectoryRenderer.positionCount = maxSteps;

        int actualSteps = 0;

        for (int i = 0; i < maxSteps; i++)
        {
            physicsScene.Simulate(Time.fixedDeltaTime);

            trajectoryRenderer.SetPosition(i, ghostObj.transform.position);
            actualSteps++;

            // Stop when object almost stops moving
            if (ghostRigidbody.IsSleeping() || ghostRigidbody.linearVelocity.sqrMagnitude < 0.01f)
            {
                RaycastHit hit;

                Vector3 start = ghostObj.transform.position;

                if (Physics.Raycast(start, Vector3.down, out hit, 10f))
                {
                    ghostStopped = true;
                    EndTrajectory(hit.point, hit.normal, ghostObj.transform.rotation);
                }

                break;
            }
        }

        trajectoryRenderer.positionCount = actualSteps;

        if (actualSteps > endPointsIgnored)
            trajectoryRenderer.positionCount = actualSteps - endPointsIgnored;

        if (!ghostStopped && landingMarker != null) landingMarker.SetActive(false);

        Destroy(ghostObj);
    }

    private void CalculateTrajectory(Vector3 pos, Vector3 velocity)
    {
        int pointsDrawn;
        int maxSteps = Mathf.CeilToInt(maxTime / Time.fixedDeltaTime);

        bool hitFound = false;

        Vector3 previousPoint = pos;

        trajectoryRenderer.positionCount = maxSteps;

        for (pointsDrawn = 0; pointsDrawn < maxSteps; pointsDrawn++)
        {
            float t = pointsDrawn * Time.fixedDeltaTime;
            Vector3 point = pos + velocity * t + 0.5f * Physics.gravity * t * t;

            Vector3 segment = point - previousPoint;
            float distance = segment.magnitude;

            trajectoryRenderer.SetPosition(pointsDrawn, point);

            if (Physics.Raycast(previousPoint, segment.normalized, out RaycastHit hit, distance))
            {
                trajectoryRenderer.SetPosition(++pointsDrawn, hit.point);
                EndTrajectory(hit.point, hit.normal);

                hitFound = true;

                break;
            }

            previousPoint = point;
        }

        trajectoryRenderer.positionCount = pointsDrawn;

        if (pointsDrawn > endPointsIgnored)
            trajectoryRenderer.positionCount = pointsDrawn - endPointsIgnored;

        if (!hitFound && landingMarker != null) landingMarker.SetActive(false);
    }

    public void ClearTrajectory()
    {
        Debug.Log("Clearing trajectory");

        trajectoryRenderer.positionCount = 0;
        landingMarker.SetActive(false);
    }

    private void EndTrajectory(Vector3 position, Vector3 normal, Quaternion? rotation = null)
    {
        if (!landingMarker ||!trajectoryRenderer.enabled || !grabbedObject)
            return;

        Collider objectCollider = grabbedObject.GetComponent<Collider>();

        if (objectCollider == null)
        {
            Debug.LogError("Grabbed object must have a BoxCollider component.");
            return;
        }
        
        landingMarker.transform.position = position; // + normal * halfHeight;

        landingMarker.SetActive(true);
    }

    void Update()
    {
        if (grabComponent.grabbedObject != null)
        {
            grabbedObject = grabComponent.grabbedObject;
        }
        else 
        {
            grabbedObject = null; 
        }

        if (charging && grabbedObject != null)
        {
            Vibrate(lowVibrationIntensity, highVibrationIntensity, vibrationDuration);
            ChargeUpdate();
        }

        if (useSimulation)
        {
            if (spawnedObjects.Count == 0) return;
            foreach (var item in spawnedObjects)
            {
                item.Value.position = item.Key.position;
                item.Value.rotation = item.Key.rotation;
            }
        }
    }
}

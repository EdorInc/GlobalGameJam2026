using UnityEngine;

namespace Gameplay.Cameras
{
    public class NonSplitCameraController : MonoBehaviour
    {
        [Header("Targets")]
        private Transform target;
        private Transform otherTarget;

        [Header("Follow")]
        [Tooltip("Base smooth time for camera movement.")]
        [SerializeField] private float smoothTime = 0.1f;

        [Header("Distances")]
        [Tooltip("Distance between players at which the camera starts widening/zooming out.")]
        [SerializeField] private float maxPlayerDistance = 8f;
        [Tooltip("Distance between players at which the camera reaches its maximum widen state.")]
        [SerializeField] private float maxWidenDistance = 14f;
        
        [Header("Height")]
        [Tooltip("Maximum height at which the camera can be placed.")]
        [SerializeField] private float maxCameraHeight = 16f;

        [Header("Widen (Position/Rotation)")]
        [Tooltip("If true, widen by moving/rotating the camera toward the max pose.")]
        [SerializeField] private bool usePositionRotationWiden = true;
        [Tooltip("Maximum camera position when fully widened.")]
        [SerializeField] private Vector3 widenedCameraPosition;
        [Tooltip("Maximum camera rotation (Euler) when fully widened.")]
        [SerializeField] private Vector3 widenedCameraEuler;

        [Header("FOV")]
        [Tooltip("If true, widen by increasing the camera FOV.")]
        [SerializeField] private bool useFovWiden = true;
        [Tooltip("Maximum FOV when fully widened.")]
        [SerializeField] private float maxFov = 70f;
        [Tooltip("Maximum orthographic size when fully widened (only for orthographic cameras).")]
        [SerializeField] private float maxOrthoSize = 12f;

        [Header("Speed")]
        [Tooltip("Minimum transition speed when players move slowly.")]
        [SerializeField] private float zoomMinSpeed = 1f;
        [Tooltip("Maximum transition speed when players move fast.")]
        [SerializeField] private float zoomMaxSpeed = 6f;

        private Camera cachedCamera;
        private Vector3 defaultPosition;
        private Quaternion defaultRotation;
        private float defaultFov;
        private float defaultOrthoSize;
        private Vector3 defaultOffset;
        private Vector3 widenedOffset;
        private bool initialized;
        private float lastDistance;
        private Vector3 positionVelocity;
        private GroundDetector ground1;
        private GroundDetector ground2;
        private bool heightUnlocked;

        private void Awake()
        {
            cachedCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (!initialized)
            {
                if (target == null || otherTarget == null)
                {
                    return;
                }
                InitializeDefaults();
            }

            Vector3 midpoint = (target.position + otherTarget.position) * 0.5f;
            float distance = Vector3.Distance(target.position, otherTarget.position);
            float widenT = GetWidenFactor(distance);
            float dynamicSpeed = GetDynamicSpeed(distance);
            float effectiveSmoothTime = Mathf.Max(0.01f, smoothTime / dynamicSpeed);

            Vector3 desiredPosition = midpoint + defaultOffset;
            desiredPosition.y = defaultPosition.y;
            Quaternion desiredRotation = defaultRotation;

            if (usePositionRotationWiden)
            {
                Vector3 desiredOffset = Vector3.Lerp(defaultOffset, widenedOffset, widenT);
                desiredPosition = midpoint + desiredOffset;
                desiredRotation = Quaternion.Slerp(defaultRotation, Quaternion.Euler(widenedCameraEuler), widenT);
            }
            
            bool bothGrounded = ground1 != null && ground2 != null && ground1.IsGrounded && ground2.IsGrounded;
            if (!heightUnlocked && bothGrounded)
            {
                heightUnlocked = true;
            }
            
            if (!heightUnlocked) 
            {
                // Mientras no estén grounded, mantener la altura inicial
                desiredPosition.y = defaultPosition.y;
            }
            else
            {
                // Una vez grounded, seguir el midpoint pero con tope de altura
                desiredPosition.y = Mathf.Min(desiredPosition.y, maxCameraHeight);
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                effectiveSmoothTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                Time.deltaTime * dynamicSpeed
            );

            if (useFovWiden && cachedCamera != null)
            {
                if (cachedCamera.orthographic)
                {
                    float desiredSize = Mathf.Lerp(defaultOrthoSize, maxOrthoSize, widenT);
                    cachedCamera.orthographicSize = Mathf.Lerp(
                        cachedCamera.orthographicSize,
                        desiredSize,
                        Time.deltaTime * dynamicSpeed
                    );
                }
                else
                {
                    float desiredFov = Mathf.Lerp(defaultFov, maxFov, widenT);
                    cachedCamera.fieldOfView = Mathf.Lerp(
                        cachedCamera.fieldOfView,
                        desiredFov,
                        Time.deltaTime * dynamicSpeed
                    );
                }
            }
        }

        public void SetTargets(Transform newTarget, Transform newOtherTarget)
        {
            target = newTarget;
            otherTarget = newOtherTarget;

            var seeThrough = GetComponent<SeeThrough>();
            if (seeThrough != null)
            {
                seeThrough.SetPlayers(target, otherTarget);
            }
            
            ground1 = target.GetComponentInParent<GroundDetector>() ?? target.GetComponentInChildren<GroundDetector>();
            ground2 = otherTarget.GetComponentInParent<GroundDetector>() ?? otherTarget.GetComponentInChildren<GroundDetector>();
            heightUnlocked = false;

            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            if (target == null || otherTarget == null)
            {
                return;
            }

            defaultPosition = transform.position;
            defaultRotation = transform.rotation;
            if (cachedCamera != null)
            {
                defaultFov = cachedCamera.fieldOfView;
                defaultOrthoSize = cachedCamera.orthographicSize;
            }

            Vector3 midpoint = (target.position + otherTarget.position) * 0.5f;
            defaultOffset = defaultPosition - midpoint;
            widenedOffset = widenedCameraPosition == Vector3.zero
                ? defaultOffset
                : widenedCameraPosition - midpoint;

            lastDistance = Vector3.Distance(target.position, otherTarget.position);
            initialized = true;
        }

        private float GetWidenFactor(float distance)
        {
            float effectiveMax = Mathf.Max(maxPlayerDistance + 0.01f, maxWidenDistance);
            return Mathf.Clamp01(Mathf.InverseLerp(maxPlayerDistance, effectiveMax, distance));
        }

        private float GetDynamicSpeed(float distance)
        {
            if (Time.deltaTime <= 0f)
            {
                return zoomMinSpeed;
            }

            float deltaDistance = (distance - lastDistance) / Time.deltaTime;
            lastDistance = distance;

            float speedT = Mathf.Clamp01(Mathf.Abs(deltaDistance) / Mathf.Max(0.01f, maxPlayerDistance));
            return Mathf.Lerp(zoomMinSpeed, zoomMaxSpeed, speedT);
        }
    }
}

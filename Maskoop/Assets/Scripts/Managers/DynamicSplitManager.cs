using UnityEngine;
using UnityEngine.UIElements; // Add this namespace

public class DynamicSplitManager : MonoBehaviour
{
    [Header("Distances")]
    public float mergeDistance = 5f;
    public float splitDistance = 7f;

    [Header("Transition Settings")]
    public float transitionSpeed = 3f; // Transition animation speed

    [Header("Zoom Settings")]
    public float zoomOutOffset = 3f;
    public float zoomSpeed = 2f;

    [Header("UI References")]
    [Tooltip("Assign the GameObject that holds the UIDocument for the Game Screen")]
    private UIDocument gameUIDocument; // Reference to access the separator

    private Transform player1;
    private Transform player2;
    private Camera camera1;
    private Camera camera2;
    private FollowPlayer followScript1;
    private FollowPlayer followScript2;

    public static bool isMerged;
    private float splitProgress = 1f; // 1 = Split, 0 = Merged
    
    private VisualElement separatorElement; // To hold the visual line

    private void Start()
    {
        FindCanvas();
        // We don't depend from Start anymore, but we try to resolve here as well.
        TryResolveSeparatorElement();
    }

    private void Update()
    {
        if (!player1 || !player2 || !camera1 || !camera2) return;

        // Measure the distance using the character's transforms (CharacterStateController), which are the ones moving
        float distance = Vector3.Distance(player1.position, player2.position);

        EvaluateMergeSplitState(distance);
        UpdateCameraTransition();

        // If the UI wasn't ready at the start, we apply the separator visibility as soon as we can resolve it.
        if (separatorElement == null && FindCanvas() && TryResolveSeparatorElement())
        {
            UpdateSeparatorVisual();
        }
    }

    public void SetupPlayers(GameObject p1Root, GameObject p2Root)
    {
        gameUIDocument = null;
        separatorElement = null;

        // Ignore the general root. Find the Transform of the child character that actually moves.
        player1 = p1Root.GetComponentInChildren<CharacterStateController>().transform;
        player2 = p2Root.GetComponentInChildren<CharacterStateController>().transform;

        // Locate cameras under the general root
        camera1 = p1Root.GetComponentInChildren<Camera>();
        camera2 = p2Root.GetComponentInChildren<Camera>();

        followScript1 = camera1.GetComponent<FollowPlayer>();
        followScript2 = camera2.GetComponent<FollowPlayer>();

        if (followScript1 != null)
        {
            followScript1.SetTargetAndInitialize(player1);
            followScript1.SetOtherTarget(player2);
            followScript1.SetZoomSettings(zoomOutOffset, zoomSpeed);
        }

        if (followScript2 != null)
        {
            followScript2.SetTargetAndInitialize(player2);
            // followScript2.SetOtherTarget(player1);
            // followScript2.SetZoomSettings(zoomOutOffset, zoomSpeed);
        }

        var listener2 = camera2.GetComponent<AudioListener>();
        if (listener2 != null) Destroy(listener2);

        // Cameras are set to see each other always
        var seeThrough1 = camera1.GetComponent<SeeThrough>();
        if (seeThrough1 != null)
        {
            seeThrough1.SetPlayers(player1, player2);
        }

        var seeThrough2 = camera2.GetComponent<SeeThrough>();
        if (seeThrough2 != null)
        {
            seeThrough2.SetPlayers(player2, player1);
        }

        float distance = Vector3.Distance(player1.position, player2.position);
        InitializeCameraState(distance);
    }

    private void InitializeCameraState(float distance)
    {
        // Consistent initial state before the first Update
        // As of now, the cameras will start merged if the players are closer than splitDistance.
        isMerged = distance < splitDistance;
        splitProgress = isMerged ? 0f : 1f;

        camera2.gameObject.SetActive(!isMerged);
        UpdateSeparatorVisual();
        ApplyCameraRects(splitProgress);
    }

    private void EvaluateMergeSplitState(float distance)
    {
        if (isMerged)
        {
            if (distance > splitDistance)
            {
                isMerged = false;

                if (followScript2 != null)
                {
                    followScript2.SnapToSplitNow();
                }

                camera2.gameObject.SetActive(true);
                UpdateSeparatorVisual();
            }
        }
        else
        {
            if (distance < mergeDistance)
            {
                isMerged = true;
                UpdateSeparatorVisual();
            }
        }
    }

    private void UpdateCameraTransition()
    {
        float targetSplit = isMerged ? 0f : 1f;
        splitProgress = Mathf.MoveTowards(splitProgress, targetSplit, Time.deltaTime * transitionSpeed);

        ApplyCameraRects(splitProgress);

        if (isMerged && splitProgress == 0f)
        {
            camera2.gameObject.SetActive(false);
        }
    }

    private void ApplyCameraRects(float progress)
    {
        camera1.rect = new Rect(0f, 0f, Mathf.Lerp(1f, 0.5f, progress), 1f);
        camera2.rect = new Rect(Mathf.Lerp(1f, 0.5f, progress), 0f, 0.5f, 1f);
    }

    private bool TryResolveSeparatorElement()
    {
        // Si ya existe y sigue vivo en un panel, reutilizar
        if (separatorElement != null && separatorElement.panel != null)
            return true;

        separatorElement = null;

        // Resolver UIDocument cuando falte
        if (gameUIDocument == null)
            gameUIDocument = FindFirstObjectByType<UIDocument>();

        if (gameUIDocument == null)
            return false;

        var root = gameUIDocument.rootVisualElement;
        if (root == null)
            return false;

        separatorElement = root.Q<VisualElement>("separator");
        return separatorElement != null;
    }

    private void UpdateSeparatorVisual()
    {
        if (!TryResolveSeparatorElement()) return;
        separatorElement.style.display = isMerged ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private bool FindCanvas()
    {
        gameUIDocument = FindFirstObjectByType<UIDocument>();
        return gameUIDocument != null;
    }
}

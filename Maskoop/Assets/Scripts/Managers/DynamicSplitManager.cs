using UnityEngine;
using UnityEngine.UIElements; // Add this namespace

public class DynamicSplitManager : MonoBehaviour
{
    [Header("Distances")]
    public float mergeDistance = 5f;
    public float splitDistance = 7f;

    [Header("Transition Settings")]
    public float transitionSpeed = 3f; // Transition animation speed

    [Header("UI References")]
    [Tooltip("Assign the GameObject that holds the UIDocument for the Game Screen")]
    public UIDocument gameUIDocument; // Reference to access the separator

    private Transform player1;
    private Transform player2;
    private Camera camera1;
    private Camera camera2;
    private FollowPlayer followScript1;
    private FollowPlayer followScript2;

    private bool isMerged = false;
    private float splitProgress = 1f; // 1 = Split, 0 = Merged
    
    private VisualElement separatorElement; // To hold the visual line

    private void Start()
    {
        // Try to get the separator from the UI
        if (gameUIDocument != null)
        {
            var root = gameUIDocument.rootVisualElement;
            separatorElement = root.Q<VisualElement>("separator");
        }
        else
        {
            Debug.LogWarning("UIDocument not assigned in DynamicSplitManager!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!player1 || !player2) return;

        // Measure the distance using the character's transforms (CharacterStateController), which are the ones moving
        float distance = Vector3.Distance(player1.position, player2.position);

        if (isMerged && distance > splitDistance)
        {
            isMerged = false;
            followScript1.isMerged = false;
            camera2.gameObject.SetActive(true);
            
            // Show the separator line
            if (separatorElement != null) separatorElement.style.display = DisplayStyle.Flex;
        }
        else if (!isMerged && distance < mergeDistance)
        {
            isMerged = true;
            followScript1.isMerged = true;
            
            // Hide the separator line
            if (separatorElement != null) separatorElement.style.display = DisplayStyle.None;
        }

        float targetSplit = isMerged ? 0f : 1f;
        splitProgress = Mathf.MoveTowards(splitProgress, targetSplit, Time.deltaTime * transitionSpeed);

        camera1.rect = new Rect(0, 0, Mathf.Lerp(1f, 0.5f, splitProgress), 1f);
        camera2.rect = new Rect(Mathf.Lerp(1f, 0.5f, splitProgress), 0, 0.5f, 1f);

        if (isMerged && splitProgress == 0f)
        {
            camera2.gameObject.SetActive(false);
        }
    }

    public void SetupPlayers(GameObject p1Root, GameObject p2Root)
    {
        // Ignore the general root. Find the Transform of the child character that actually moves.
        player1 = p1Root.GetComponentInChildren<CharacterStateController>().transform;
        player2 = p2Root.GetComponentInChildren<CharacterStateController>().transform;

        // Locate cameras under the general root
        camera1 = p1Root.GetComponentInChildren<Camera>();
        camera2 = p2Root.GetComponentInChildren<Camera>();

        camera1.rect = new Rect(0, 0, 0.5f, 1f);
        camera2.rect = new Rect(0.5f, 0, 0.5f, 1f);

        Destroy(camera2.GetComponent<AudioListener>());

        followScript1 = camera1.GetComponent<FollowPlayer>();
        followScript2 = camera2.GetComponent<FollowPlayer>();

        // Inform the FollowPlayer script which transform it needs to track
        // (Assign the target itself in case it wasn't hooked up in the Inspector)
        followScript1.target = player1;
        followScript2.target = player2;

        followScript1.SetOtherTarget(player2);
        followScript2.SetOtherTarget(player1);
    }
}

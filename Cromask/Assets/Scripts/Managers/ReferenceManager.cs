using UnityEngine;
using UnityEngine.InputSystem;

public class ReferenceManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private GameObject playerOneReference;

    [SerializeField]
    private GameObject playerTwoReference;

    [SerializeField]
    private Camera playerOneCamera;

    [SerializeField]
    private Camera playerTwoCamera;

    private Gamepad playerOneGamepad;
    private Gamepad playerTwoGamepad;

    [SerializeField]
    private GameObject redMask;

    [SerializeField]
    private GameObject blueMask;

    [SerializeField]
    private GameObject greenMask;

    private static ReferenceManager _instance;

    public static ReferenceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject("ReferenceManager");
                _instance = singletonObject.AddComponent<ReferenceManager>();
                //DontDestroyOnLoad(singletonObject);
            }
            return _instance;
        }
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayerInput playerOneInput = playerOneReference.GetComponent<PlayerInput>();
        if(playerOneInput.devices.Count > 0)
        {
            Gamepad gamepad = playerOneReference.GetComponent<PlayerInput>().devices[0] as Gamepad;
            playerOneGamepad = gamepad;
        }

        PlayerInput playerTwoInput = playerTwoReference.GetComponent<PlayerInput>();
        if (playerTwoInput.devices.Count > 0)
        {
            Gamepad gamepad = playerTwoReference.GetComponent<PlayerInput>().devices[0] as Gamepad;
            playerTwoGamepad = gamepad;
        }

    }
    public GameObject GetPlayerOne() => playerOneReference;
    public GameObject GetPlayerTwo() => playerTwoReference;
    public Camera GetPlayerOneCamera() => playerOneCamera;
    public Camera GetPlayerTwoCamera() => playerTwoCamera;
    public GameObject GetRedMask() => redMask;
    public GameObject GetBlueMask() => blueMask;
    public GameObject GetGreenMask() => greenMask;

    public MaskManager GetPlayerOneMask() =>
    playerOneReference != null ? playerOneReference.GetComponent<MaskManager>() : null;

    public MaskManager GetPlayerTwoMask() =>
        playerTwoReference != null ? playerTwoReference.GetComponent<MaskManager>() : null;

    public Gamepad GetPlayerOneGamepad() => playerOneGamepad;
    public Gamepad GetPlayerTwoGamepad() => playerTwoGamepad;
}

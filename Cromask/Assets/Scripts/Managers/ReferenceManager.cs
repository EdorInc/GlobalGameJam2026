using UnityEngine;

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

    private static ReferenceManager _instance;

    public static ReferenceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject("ReferenceManager");
                _instance = singletonObject.AddComponent<ReferenceManager>();
                DontDestroyOnLoad(singletonObject);
            }
            return _instance;
        }
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    public GameObject GetPlayerOne() => playerOneReference;
    public GameObject GetPlayerTwo() => playerTwoReference;
    public Camera GetPlayerOneCamera() => playerOneCamera;
    public Camera GetPlayerTwoCamera() => playerTwoCamera;
}

using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Conection Settings")]
    [SerializeField]
    [Tooltip("Channel to use to connect to buttons. Buttons need to have the same channel to open the door/Bridge")]
    private int channel = 1;
    [SerializeField] private int buttonsNedded = 1;

    [Header("Position Settings")]
    [SerializeField]
    [Tooltip("Position of the door/bridge when it opens")]
    private Transform openPosition;

    [Header("Movement Settings")]
    [SerializeField]
    [Tooltip("Speed to open the door/bridge")]
    private float speed = 2;

    private bool shouldOpen = false;

    private int buttonsLeft = 1;
    void Start()
    {
        //Link the function to the unity action emmited by targets and buttons
        EventManager.OnButtonPressed += OnButtonPressRecived;
        buttonsLeft = buttonsNedded;
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldOpen)
        {
            transform.position = Vector3.MoveTowards(transform.position, openPosition.position, Time.deltaTime * speed);
        }
        if (transform.position.Equals(openPosition.position))
        {
            shouldOpen = false;
        }
    }

    void OnButtonPressRecived(int channel)
    {
        if(this.channel == channel)
        {
            buttonsLeft--;
            if(buttonsLeft == 0)
            {
                shouldOpen = true;
            }
        }
    }
}

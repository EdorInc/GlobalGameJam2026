using UnityEngine;

public abstract class ActivableBase : MonoBehaviour
{
    [Header("Conection Settings")]
    [SerializeField]
    [Tooltip("Channel to use to connect to buttons. Buttons need to have the same channel to activate this object")]
    protected int channel = 1;
    [SerializeField]
    [Tooltip("Amount of activators needed to activate this object")]
    protected int activatorsNeeded = 1;

    protected int activatorsLeft = 1;

    protected bool shouldActivate = false;


    void Start()
    {
        //Link the function to the unity action emmited by targets and buttons
        activatorsLeft = activatorsNeeded;
    }

    private void OnEnable()
    {
        EventManager.OnButtonPressed += OnActivatorRecived; 
    }

    private void OnDisable()
    {
        EventManager.OnButtonPressed -= OnActivatorRecived;
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldActivate)
        {
            ActivatedAction();

            if (StopCondition())
            {
                shouldActivate = false;
            }
        }
    }

    public void OnActivatorRecived(int channel)
    {
        if (this.channel == channel)
        {
            activatorsNeeded--;
            Debug.Log("Activating channel " + channel + " on object " + gameObject.name);

            if (activatorsNeeded == 0)
            {
                shouldActivate = true;
                Debug.Log("All activators received for channel " + channel + " on object " + gameObject.name);
            }
        }
    }

    protected abstract bool StopCondition();
    protected abstract void ActivatedAction();
     
}

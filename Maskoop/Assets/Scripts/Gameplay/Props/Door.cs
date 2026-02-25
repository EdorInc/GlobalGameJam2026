using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Conection Settings")]
    [SerializeField] private int channel = 1;
    [SerializeField] private int buttonsNedded = 1;

    private int buttonsLeft = 1;
    void Start()
    {
        EventManager.OnButtonPressed += OnButtonPressRecived;
        buttonsLeft = buttonsNedded;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnButtonPressRecived(int channel)
    {
        if(this.channel == channel)
        {
            buttonsLeft--;
            if(buttonsLeft == 0)
            {
                Debug.Log("Abrir");
            }
        }
    }
}

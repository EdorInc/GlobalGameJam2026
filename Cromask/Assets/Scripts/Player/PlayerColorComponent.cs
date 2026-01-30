using UnityEngine;

public class PlayerColorComponent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private ObjectColor playerColor = ObjectColor.Red;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ObjectColor GetPlayerColor() => playerColor;
}

using UnityEngine;
using UnityEngine.InputSystem;

public class LightLogic : MonoBehaviour
{
    [SerializeField]
    private GameObject sceneLight;
    public void SetLightState(bool state)
    {
        sceneLight.SetActive(state);
    }


    private void OnTriggerEnter(Collider other)
    {
       
        if (other.gameObject.GetComponent<PlayerInput>())
        {
            SetLightState(true);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
       
        if (other.gameObject.GetComponent<PlayerInput>())
        {
            SetLightState(false);
        }
          
    }

    
    private void OnDisable()
    {
        SetLightState(false);
    }
}

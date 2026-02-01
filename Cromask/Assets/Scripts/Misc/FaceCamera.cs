using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField]
    private Transform cam;

    private void Awake()
    {
        
    }

    private void LateUpdate()
    {
        Vector3 direction = transform.position - cam.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}

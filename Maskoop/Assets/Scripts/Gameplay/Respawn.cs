using UnityEngine;

public class Respawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Vector3 respawnPosition = new Vector3(0, 1, 0);
    public float voidDistance = -3;
    public bool willDestroy = false;

    new private Rigidbody rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rigidbody.position.y < voidDistance)
        {
            if (willDestroy == false)
            {
                RespawnFunction();
            }
            else
            {
                Destroy();
            }
        }
        
    }

    void RespawnFunction()
    {
        rigidbody.position = respawnPosition;
    }

    void Destroy()
    {
        Destroy(gameObject);
    }
}

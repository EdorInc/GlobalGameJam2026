using UnityEngine;

public class Reset : MonoBehaviour
{

    [SerializeField]
    private float respawnHeight = 10.0f;

    private Vector3 lastValidPosition;

    void Awake()
    {
        lastValidPosition = transform.position + Vector3.up * respawnHeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZzzzone"))
        {
            RespawnObject();
        }
    }

    public void ForceRespawn()
    {
        RespawnObject();
    }

    private void RespawnObject()
    {
        if (lastValidPosition != null)
        {
            transform.position = lastValidPosition;
            Debug.Log("Objeto respawneado en la última posición válida");
        }
    }
}

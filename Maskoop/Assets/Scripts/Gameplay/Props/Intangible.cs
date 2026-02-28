using UnityEngine;

public class Intangible : MonoBehaviour
{
    private void Start()
    {
        Collider myCollider = GetComponent<Collider>();

        // Find all objects tagged "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null)
                Physics.IgnoreCollision(myCollider, playerCollider);

            Collider[] playerChildColliders = player.GetComponentsInChildren<Collider>();
            foreach (Collider childCollider in playerChildColliders)
            {
                if (childCollider != playerCollider)
                    Physics.IgnoreCollision(myCollider, childCollider);
            }
        }
    }
}

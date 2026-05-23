using UnityEngine;

public class OnFire : MonoBehaviour
{
    [Header("Is On Fire Settings")]
    [SerializeField] private bool IsOnFire;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && IsOnFire)
        {
            CharacterStateController state = other.GetComponent<CharacterStateController>();

            if (state != null)
            {
                BaseMask currentMask = state.GetCurrentMask();

                bool hasFireMask = currentMask is FireMask;

                if (!hasFireMask)
                {
                    EventManager.OnLitOnFire?.Invoke(other);
                }
            }
        }
    }
}

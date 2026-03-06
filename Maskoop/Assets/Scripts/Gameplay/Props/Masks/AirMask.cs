using UnityEngine;
[DefaultExecutionOrder(300)]
public class AirMask : BaseMask
{
    [Header("Flutter Settings")]
    [Tooltip("Reduction in speed of the flutter")]
    [SerializeField] private float flutterSpeedReduction = 0;
    [Tooltip("Time the player remains in the flutter state")]
    [SerializeField] private float flutterTime = 2;



    private bool IsFluttering = false;
    private Rigidbody playerRigidBody;

    private float currentFloatTime = 0;

    public override void OnUnequip()
    {
        Debug.Log("Me quito la mascara verde");
        EventManager.OnFallStarted -= StartFlutter;
        EventManager.OnFallEnded -= EndFlutter;
    }

    public override void UpdateLogic()
    {
    }

    public override void OnEquip(CharacterStateController characterState)
    {
        base.OnEquip(characterState);
        Debug.Log("Tengo la mascara verde");

        EventManager.OnFallStarted += StartFlutter;
        EventManager.OnFallEnded += EndFlutter;
    }

    public void StartFlutter(GameObject target)
    {
        if (characterState.IsMyPlayer(target))
        {
            playerRigidBody = target.GetComponent<Rigidbody>();
            IsFluttering = true;
            playerRigidBody.useGravity = false;
            characterState.IsFloating = true;
        }
    }

    public void EndFlutter(GameObject target)
    {
        if (characterState.IsMyPlayer(target))
        {
            ResetFlutter();
            playerRigidBody = null;
        }
    }

    private void ResetFlutter()
    {
        IsFluttering = false;
        currentFloatTime = 0;
        playerRigidBody.useGravity = true;
        characterState.IsFloating = false;
    }

    public override void FixedUpdateLogic()
    {
        if (IsFluttering)
        {
            Vector3 velocity = playerRigidBody.linearVelocity;
            velocity.y = 0;
            playerRigidBody.linearVelocity = velocity * 3f;
            currentFloatTime += Time.deltaTime;
        }
        if (currentFloatTime > flutterTime)
        {
            ResetFlutter();
        }
        Debug.Log("Update la mascara verde");
    }
}

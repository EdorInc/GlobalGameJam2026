using UnityEngine;
using UnityEngine.Splines;

public class WaterMask : BaseMask
{
    [Header("Material Settings")]
    public Material waterMaterial;
    private Material previousMaterial;

    private SplineContainer currentSpline;

    private bool movingInPipe = false;
    private float positionInPipe = 0;
    private Rigidbody playerRb;
    private CharacterMovementController characteMovement;
    public override void OnEquip(CharacterStateController characterState)
    {
        base.OnEquip(characterState);

        previousMaterial = characterState.GetBodyRenderer().material;

        characterState.GetBodyRenderer().material = waterMaterial;

        EventManager.OnPipeEntryPoint += PipeEntered;
    }

    public override void OnUnequip()
    {
        characterState.GetBodyRenderer().material = previousMaterial;
        EventManager.OnPipeEntryPoint -= PipeEntered;
    }

    public override void UpdateLogic()
    {

    }

    public override void FixedUpdateLogic()
    {
        if (!movingInPipe || currentSpline == null) return;

        positionInPipe += 0.3f * Time.fixedDeltaTime;

        if(positionInPipe > 1)
        {
            ExitPipe();
        }

        Vector3 position = currentSpline.EvaluatePosition(positionInPipe);
        Vector3 forward = currentSpline.EvaluateTangent(positionInPipe);

        playerRb.MovePosition(position);
        playerRb.MoveRotation(Quaternion.LookRotation(forward));
    }

    private void PipeEntered(Collider player,SplineContainer spline)
    {
        if (characterState.IsMyPlayer(player.gameObject))
        {
            if (spline && movingInPipe)
            {
                ExitPipe();
            }
            else 
            {
                EnterPipe( player,  spline);
            }
        }
    }

    private void ExitPipe()
    {
        movingInPipe = false;
        currentSpline = null;
        playerRb = null;
        characteMovement.enabled = true;
        playerRb.isKinematic = false;
        playerRb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void EnterPipe(Collider player, SplineContainer spline)
    {
        movingInPipe = true;
        currentSpline = spline;
        positionInPipe = 0;
        playerRb = player.gameObject.GetComponent<Rigidbody>();
        characteMovement = player.gameObject.GetComponent<CharacterMovementController>();
        characteMovement.enabled = false;
        playerRb.isKinematic = true;
        playerRb.interpolation = RigidbodyInterpolation.None;
    }
}

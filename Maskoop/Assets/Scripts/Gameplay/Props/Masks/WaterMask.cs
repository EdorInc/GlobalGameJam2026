using UnityEngine;
using UnityEngine.Splines;

public class WaterMask : BaseMask
{
    [Header("Material Settings")]
    public Material waterMaterial;
    public Renderer maskRenderer;
    private Material previousMaterial;

    private SplineContainer currentSpline;

    private bool movingInPipe = false;
    private float positionInPipe = 0;
    private Rigidbody playerRb;
    private CharacterMovementController characteMovement;

    private bool directMovement = true;
    public override void OnEquip(CharacterStateController characterState)
    {
        base.OnEquip(characterState);

        previousMaterial = characterState.GetBodyRenderer().material;

        characterState.GetBodyRenderer().material = waterMaterial;

        EventManager.OnWaterWall += GoThroughWall;
        EventManager.OnPipeEntryPoint += PipeEntered;
    }

    public override void OnUnequip()
    {
        characterState.GetBodyRenderer().material = previousMaterial;
        EventManager.OnPipeEntryPoint -= PipeEntered;
        EventManager.OnWaterWall -= GoThroughWall;
    }

    public override void UpdateLogic()
    {

    }

    public override void FixedUpdateLogic()
    {
        if (!movingInPipe || currentSpline == null) return;

        if (directMovement)
        {
            positionInPipe += 0.3f * Time.fixedDeltaTime;

            if (positionInPipe > 1)
            {
                ExitPipe();
                return;
            }
        }
        else
        {
            positionInPipe -= 0.3f * Time.fixedDeltaTime;

            if (positionInPipe < 0)
            {
                ExitPipe();
                return;
            }
        }

        Vector3 position = currentSpline.EvaluatePosition(positionInPipe);
        Vector3 forward = currentSpline.EvaluateTangent(positionInPipe);

        playerRb.MovePosition(position);
        playerRb.MoveRotation(Quaternion.LookRotation(forward));
    }

    private void PipeEntered(Collider player,SplineContainer spline,bool entry)
    {
        if (characterState.IsMyPlayer(player.gameObject))
        {
            if (!currentSpline && !movingInPipe)
            {
                EnterPipe(player, spline);
                directMovement = entry;
                if (!directMovement)
                {
                    positionInPipe = 1;
                }
                else
                {
                    positionInPipe = 0;
                }
            }
        }
    }

    private void ExitPipe()
    {
        movingInPipe = false;
        currentSpline = null;   
        characteMovement.enabled = true;
        playerRb.isKinematic = false;
        playerRb.interpolation = RigidbodyInterpolation.Interpolate;
        playerRb = null;
        characterState.BodyRenderer.enabled = true;
        maskRenderer.enabled = false;
    }

    private void EnterPipe(Collider player, SplineContainer spline)
    {
        movingInPipe = true;
        currentSpline = spline;
        playerRb = player.gameObject.GetComponent<Rigidbody>();
        characteMovement = player.gameObject.GetComponent<CharacterMovementController>();
        characteMovement.enabled = false;
        playerRb.isKinematic = true;
        playerRb.interpolation = RigidbodyInterpolation.None;
        characterState.BodyRenderer.enabled = false;
        maskRenderer.enabled = false;
    }


    private void GoThroughWall(Collision collision, Transform wallTransform)
    {
        Debug.Log("MUROOOOOOO");


        Vector3 playerPosition = collision.gameObject.transform.position;
        Vector3 wallPosition = collision.contacts[0].point;

        Vector3 dir = wallPosition - playerPosition;
        float distance  = Vector3.Distance(playerPosition, wallPosition);

        Vector3 newPosition = wallPosition + dir * distance * 2;

        collision.gameObject.transform.position = new Vector3( newPosition.x, playerPosition.y, newPosition.z);

    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class WaterMask : BaseMask
{
    [Header("Material Settings")]
    public Material waterMaterial;
    public Renderer maskRenderer;
    public GameObject playerInPipePrefab;
    public float speed = 20;

    private Material previousMaterial;
    private GameObject playerInTubeObject;

    private SplineContainer currentSpline;

    private List<Vector3> tubePositions;

    private bool movingInPipe = false;
    private int tubeIndex = 0;
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

        Vector3 actualPosition = playerRb.position;

        if (directMovement)
        {
            if (tubeIndex > tubePositions.Count - 1)
            {
                ExitPipe();
                return;
            }

            

            actualPosition = Vector3.MoveTowards(actualPosition, tubePositions[tubeIndex], Time.deltaTime * speed);

            if(Vector3.Distance(actualPosition, tubePositions[tubeIndex]) < 0.1f)
            {
                tubeIndex++;
            }
        }
        else
        {
            if (tubeIndex < 0)
            {
                ExitPipe();
                return;
            }

            actualPosition = Vector3.MoveTowards(actualPosition, tubePositions[tubeIndex], Time.deltaTime * speed);

            if (Vector3.Distance(actualPosition, tubePositions[tubeIndex]) < 0.1f)
            {
                tubeIndex--;
            }
        }

        playerRb.MovePosition(actualPosition);
    }

    private void PipeEntered(Collider player, TubeSpawner spline,bool entry)
    {
        if (characterState.IsMyPlayer(player.gameObject))
        {
            if (!currentSpline && !movingInPipe)
            {
                EnterPipe(player, spline);
                directMovement = entry;
                if (!directMovement)
                {
                    tubeIndex = tubePositions.Count - 1;
                }
                else
                {
                    
                    tubeIndex = 0;
                }
                
                playerRb.position = tubePositions[tubeIndex];
                Quaternion rot = Quaternion.Euler(new Vector3(0, 0, 180));
                playerInTubeObject = Instantiate(playerInPipePrefab, playerRb.position + Vector3.up, rot, playerRb.gameObject.transform);
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
        playerRb.useGravity = true;
        playerRb = null;
        characterState.EnableRenderer();
        maskRenderer.enabled = true;
        

        Destroy(playerInTubeObject);
    }

    private void EnterPipe(Collider player, TubeSpawner spline)
    {
        movingInPipe = true;
        currentSpline = spline.spline;
        playerRb = player.gameObject.GetComponent<Rigidbody>();
        characteMovement = player.gameObject.GetComponent<CharacterMovementController>();
        characteMovement.enabled = false;
        playerRb.isKinematic = true;
        playerRb.interpolation = RigidbodyInterpolation.None;
        characterState.DisableRender();
        maskRenderer.enabled = false;
        playerRb.useGravity = false;
        tubePositions = spline.positionList;
        
        
    }


    private void GoThroughWall(Collision collision, Transform wallTransform)
    {

        Vector3 playerPosition = collision.gameObject.transform.position;
        Vector3 wallPosition = collision.contacts[0].point;

        Vector3 dir = wallPosition - playerPosition;
        float distance  = Vector3.Distance(playerPosition, wallPosition);

        Vector3 newPosition = wallPosition + dir * distance * 2;

        collision.gameObject.transform.position = new Vector3( newPosition.x, playerPosition.y, newPosition.z);

    }

    private void StartAnimation()
    {
        characterState.DisableRender();
    }
}

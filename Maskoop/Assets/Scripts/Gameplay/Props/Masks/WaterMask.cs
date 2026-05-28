using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[DefaultExecutionOrder(300)]
public class WaterMask : BaseMask
{
    [Header("Material Settings")]
    public Material waterMaterial;
    public Renderer maskRenderer;
    public GameObject playerInPipePrefab;
    public float speed = 20f;

    private Material previousMaterial;
    private GameObject playerInTubeObject;
    private List<Vector3> tubePositions;

    private bool movingInPipe;
    private bool exitingPipe;

    private int tubeIndex;

    private bool directMovement = true;

    private Rigidbody playerRb;
    private CharacterMovementController characterMovement;

    private Vector3 lastValidDirection;
    private Vector3 exitDirection;
    private Vector3 exitTarget;

    [SerializeField] private float exitOffset = 2f;

    public override void OnEquip(CharacterStateController characterState)
    {
        base.OnEquip(characterState);

        SetupMaterial();
        SubscribeEvents();
        IgnoreAllWaterWalls(true);
    }

    public override void OnUnequip()
    {
        RestoreMaterial();
        UnsubscribeEvents();
        IgnoreAllWaterWalls(false);
    }

    public override void UpdateLogic()
    {
    }

    public override void FixedUpdateLogic()
    {
        if (exitingPipe)
        {
            UpdateExitMovement();
            return;
        }

        if (movingInPipe == false)
        {
            return;
        }

        UpdatePipeMovement();
    }

    private void SetupMaterial()
    {
        previousMaterial = characterState.GetBodyRenderer().material;
        characterState.GetBodyRenderer().material = waterMaterial;
    }

    private void RestoreMaterial()
    {
        characterState.GetBodyRenderer().material = previousMaterial;
    }

    private void SubscribeEvents()
    {
        EventManager.OnWaterWallEnter += EnterWaterWall;
        EventManager.OnWaterWallExit += ExitWaterWall;
        EventManager.OnPipeEntryPoint += PipeEntered;
    }

    private void UnsubscribeEvents()
    {
        EventManager.OnWaterWallEnter -= EnterWaterWall;
        EventManager.OnWaterWallExit -= ExitWaterWall;
        EventManager.OnPipeEntryPoint -= PipeEntered;
    }

    private void IgnoreAllWaterWalls(bool ignore)
    {
        Collider playerCollider = characterState.GetComponent<Collider>();

        WaterWall[] walls = FindObjectsByType<WaterWall>(FindObjectsSortMode.InstanceID);

        for (int i = 0; i < walls.Length; i++)
        {
            Collider wallCollider = walls[i].GetComponent<Collider>();
            Physics.IgnoreCollision(playerCollider, wallCollider, ignore);
        }
    }

    private void UpdatePipeMovement()
    {
        if (HasReachedPipeEnd())
        {
            StartExit();
            return;
        }

        Vector3 targetPosition = tubePositions[tubeIndex];

        Vector3 direction = (targetPosition - playerRb.position);

        if (direction.sqrMagnitude > 0.0001f)
        {
            direction = direction.normalized;
            lastValidDirection = direction;
        }
        else
        {
            direction = lastValidDirection;
        }

        MovePlayer(direction);

        UpdateVisuals(direction);

        CheckAdvanceNode(targetPosition);

    }

    private void UpdateExitMovement()
    {
        Vector3 direction = (exitTarget - playerRb.position).normalized;

        MovePlayer(direction);

        UpdateVisuals(direction);

        if (Vector3.Distance(playerRb.position, exitTarget) < 0.2f)
        {
            FinishExit();
        }
    }

    private bool HasReachedPipeEnd()
    {
        if (directMovement == true)
        {
            return tubeIndex >= tubePositions.Count;
        }

        return tubeIndex < 0;
    }

    private void MovePlayer(Vector3 direction)
    {
        playerRb.linearVelocity = direction * speed;
    }

    private void UpdateVisuals(Vector3 direction)
    {
        playerInTubeObject.transform.position = playerRb.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            playerInTubeObject.transform.rotation =
                Quaternion.Slerp(
                    playerInTubeObject.transform.rotation,
                    targetRotation,
                    Time.fixedDeltaTime * 15f
                );
        }
    }

    private void CheckAdvanceNode(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(playerRb.position, targetPosition);

        if (distance > 0.1f)
        {
            return;
        }

        if (directMovement == true)
        {
            tubeIndex++;
            return;
        }

        tubeIndex--;
    }

    private void StartExit()
    {
        if (exitingPipe == true)
        {
            return;
        }

        exitingPipe = true;

        Vector3 safeDirection = lastValidDirection;

        if (safeDirection.sqrMagnitude < 0.001f)
        {
            safeDirection = Vector3.forward;
        }

        exitTarget = playerRb.position + safeDirection * exitOffset;
    }

    private void FinishExit()
    {
        exitingPipe = false;
        movingInPipe = false;

        characterMovement.enabled = true;

        playerRb.interpolation = RigidbodyInterpolation.Interpolate;
        playerRb.useGravity = true;

        playerRb.gameObject.GetComponent<CapsuleCollider>().enabled = true;

        characterState.EnableRenderer();
        maskRenderer.enabled = true;

        Destroy(playerInTubeObject);

        playerRb = null;
        characterMovement = null;
    }

    private void PipeEntered(Collider player, List<Vector3> pipe, bool entry)
    {
        if (characterState.IsMyPlayer(player.gameObject) == false)
        {
            return;
        }

        if (movingInPipe == true)
        {
            return;
        }

        EnterPipe(player, pipe, entry);
    }

    private void EnterPipe(Collider player, List<Vector3> pipe, bool entry)
    {
        movingInPipe = true;

        directMovement = entry;

        tubePositions = pipe;

        tubeIndex = directMovement ? 0 : tubePositions.Count - 1;

        playerRb = player.gameObject.GetComponent<Rigidbody>();
        characterMovement = player.gameObject.GetComponent<CharacterMovementController>();

        characterMovement.enabled = false;

        playerRb.linearVelocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;

        playerRb.interpolation = RigidbodyInterpolation.None;
        playerRb.useGravity = false;

        player.gameObject.GetComponent<CapsuleCollider>().enabled = false;

        characterState.ReleaseHeldObject();
        characterState.DisableRender();

        maskRenderer.enabled = false;

        playerRb.position = tubePositions[tubeIndex];

        playerInTubeObject =
            Instantiate(playerInPipePrefab, playerRb.position, Quaternion.identity);
    }

    private void EnterWaterWall(Collider player, float multiplier)
    {
        if (characterState.IsMyPlayer(player.gameObject) == false)
        {
            return;
        }

        characterMovement = player.GetComponent<CharacterMovementController>();

        characterState.ReleaseHeldObject();

        characterMovement.SetSpeedMultiplier(multiplier);
    }

    private void ExitWaterWall(Collider player)
    {
        if (characterState.IsMyPlayer(player.gameObject) == false)
        {
            return;
        }

        characterMovement.SetSpeedMultiplier(1f);
        characterMovement = null;
    }
}
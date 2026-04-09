using System;
using UnityEngine;

public abstract class BaseMask : MonoBehaviour
{
    protected CharacterStateController characterState;
    private Respawn respawnComponent;

    [Header("Respawn Settings")]
    [Tooltip("Time before the mask starts to flicker")]
    [SerializeField]
    private float timeBeforeRespawn = 5f;
    [Header("Flickering Settings")]
    [Tooltip("Time the mask stays flickering before respawning")]
    [SerializeField]
    private float timeFlickering = 2f;
    [Tooltip("Time the mask is invisible while flickering it will decrease with time")]
    [SerializeField]
    private float baseflickerInterval = 0.2f;


    private float currentRespawnTime = 0;
    private bool isFlickering = false;
    private Quaternion baseRotation;
    private float blinkTimer = 0;
    private bool isBlinkOn = false;
    private Renderer[] maskRenderers;
    private GroundDetector groundDetector;

    private void Start()
    {
        respawnComponent = GetComponent<Respawn>();
        groundDetector = GetComponent<GroundDetector>();
        respawnComponent.respawnPosition = transform.position;
        respawnComponent.respawnRotation = transform.rotation;
        baseRotation = transform.rotation;

        maskRenderers = GetComponentsInChildren<Renderer>();
    }

    private void Update()
    {
        if (!characterState && groundDetector.IsGrounded)
        {
            if(transform.position.x != respawnComponent.respawnPosition.x && transform.position.z != respawnComponent.respawnPosition.z)
            {
                currentRespawnTime += Time.deltaTime;
                if(currentRespawnTime > timeBeforeRespawn && !isFlickering)
                {
                    isFlickering = true;
                    currentRespawnTime = 0;
                }
                else if(currentRespawnTime > timeFlickering && isFlickering)
                {
                    isFlickering = false;
                    respawnComponent.RespawnFunction();
                    SetMaskVisible(true);
                }
                if (isFlickering)
                {
                    HandleBlinking();
                }
            }
        }
    }

    public abstract void UpdateLogic();

    public abstract void FixedUpdateLogic();

    public virtual void OnEquip(CharacterStateController characterState)
    {
        this.characterState = characterState;
        currentRespawnTime = 0;
        isFlickering = false;
    }

    public virtual void OnUnequip()
    {
        characterState = null;
    }

    public virtual void Respawn()
    {
        respawnComponent.RespawnFunction();
    }

    private void HandleBlinking()
    {
        blinkTimer -= Time.deltaTime;

        if (blinkTimer <= 0f)
        {
            float progress = currentRespawnTime / timeFlickering;

            float currentInterval = Mathf.Lerp(baseflickerInterval, 0.02f, progress);

            blinkTimer = currentInterval;

            isBlinkOn = !isBlinkOn;

            SetMaskVisible(isBlinkOn);
        }
    }

    private void SetMaskVisible(bool visible)
    {
        foreach (var renderer in maskRenderers)
        {
            renderer.enabled = visible;
        }
    }
}

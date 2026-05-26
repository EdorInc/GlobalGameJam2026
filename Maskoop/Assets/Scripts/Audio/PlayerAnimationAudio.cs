using UnityEngine;
using FMODUnity;

public class PlayerAnimationAudio : MonoBehaviour
{
    [SerializeField]
    private float fallSFXVelocity = -3f;

    [SerializeField]
    private float impactCooldown = 0.2f;
    private float lastImpactTime = 0f;

    private GroundDetector groundDetector;
    private CharacterStateController stateController;
    private Rigidbody rb;

    private bool fallingSoundPlaying = false;
    private bool checkingForFallingSound = false;

    private void Awake()
    {
        if (transform.parent != null)
        {
            groundDetector = transform.parent.GetComponentInChildren<GroundDetector>();
            stateController = transform.parent.GetComponentInChildren<CharacterStateController>();
            rb = transform.parent.GetComponentInChildren<Rigidbody>();
        }

        if (groundDetector == null)
        {
            Debug.LogWarning("No GroundDetector found");
        }

    }

    private void FixedUpdate()
    {
        if (checkingForFallingSound)
            CheckFallVelocityForSound();
    }

    private void OnEnable()
    {
        EventManager.OnFallStarted += PlayFallSound;
        EventManager.OnFallEnded += PlayFallImpactSound;
    }
    private void OnDisable()
    {
        EventManager.OnFallStarted -= PlayFallSound;
        EventManager.OnFallEnded -= PlayFallImpactSound;
    }
    public void PlayFootstep()
    {
        if (AudioSystem.SoundLibrary != null && AudioSystem.SoundLibrary.footstep != null)
        {
            if(groundDetector.IsGrounded)
                AudioSystem.PlaySFX(AudioSystem.SoundLibrary?.footstep, transform.position);
        }
        else
        {
            Debug.LogWarning("AudioSystem: Footstep soundlibrary not found.");
        }
    }

    public void CheckFallVelocityForSound()
    {
        if (fallingSoundPlaying)
            return;


        if (stateController != null && rb != null)
        {
            BaseMask mask = stateController.GetCurrentMask();
            if (string.Equals(mask?.name, "GreenMask"))
                return;

            if (rb.linearVelocity.y > fallSFXVelocity) //Only play if falling fast enough
                return;


            AudioSystem.PlayDynamicSFX(AudioSystem.SoundLibrary?.falling, transform.position, stateController.CharacterId);

            fallingSoundPlaying = true;
        }
    }

    public void PlayFallSound(GameObject obj)
    {
        if (obj != transform.parent.gameObject)
            return;

        checkingForFallingSound = true;
    }

    public void PlayFallImpactSound(GameObject obj)
    {
        if (obj != transform.parent.gameObject)
            return;

        //Cooldown 
        if (Time.time < lastImpactTime + impactCooldown)
            return;

        if (stateController != null)
            AudioSystem.StopDynamicSFX(AudioSystem.SoundLibrary?.falling, stateController.CharacterId);

        checkingForFallingSound = false;
        fallingSoundPlaying = false;
        AudioSystem.PlaySFX(AudioSystem.SoundLibrary?.fallImpact, transform.position);

        lastImpactTime = Time.time;
    }

}

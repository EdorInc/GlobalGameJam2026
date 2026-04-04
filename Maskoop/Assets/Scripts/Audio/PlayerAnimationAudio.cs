using UnityEngine;
using FMODUnity;

public class PlayerAnimationAudio : MonoBehaviour
{

    private GroundDetector groundDetector;
    private CharacterStateController stateController;

    private void Awake()
    {
        if (transform.parent != null)
        {
            groundDetector = transform.parent.GetComponentInChildren<GroundDetector>();
            stateController = transform.parent.GetComponentInChildren<CharacterStateController>();
        }

        if (groundDetector == null)
        {
            Debug.LogWarning("No GroundDetector found");
        }
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

    public void PlayFallSound(GameObject obj)
    {

        if (stateController != null)
        {
            BaseMask mask = stateController.GetCurrentMask();
            if(!string.Equals(mask?.name, "GreenMask"))
                AudioSystem.PlayDynamicSFX(AudioSystem.SoundLibrary?.falling, transform.position, stateController.characterId);
        }
    }

    public void PlayFallImpactSound(GameObject obj)
    {
        if (stateController != null)
            AudioSystem.StopDynamicSFX(AudioSystem.SoundLibrary?.falling, stateController.characterId);
        AudioSystem.PlaySFX(AudioSystem.SoundLibrary?.fallImpact, transform.position);
    }

}

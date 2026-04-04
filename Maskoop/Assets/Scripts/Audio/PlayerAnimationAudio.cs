using UnityEngine;
using FMODUnity;

public class PlayerAnimationAudio : MonoBehaviour
{


    public void PlayFootstep()
    {
        if (AudioSystem.SoundLibrary != null && AudioSystem.SoundLibrary.footstep != null)
        {
            AudioSystem.PlaySFX(AudioSystem.SoundLibrary?.footstep, transform.position);
        }
        else
        {
            Debug.LogWarning("AudioSystem: No se encontró la SoundLibrary o el sonido de Footstep.");
        }
    }

}

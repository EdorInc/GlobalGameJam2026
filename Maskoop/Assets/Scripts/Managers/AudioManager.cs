using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    [Header("Channels")]
    [SerializeField] private AudioEventChannel sfxChannel;
    [SerializeField] private AudioEventChannel musicChannel;

    [Header("Sound Library")]
    [SerializeField] private SoundLibrary soundLibrary;

    private EventInstance musicInstance;


    private void Awake()
    {
        AudioSystem.Initialize(sfxChannel, musicChannel, soundLibrary);
    }
    private void OnEnable()
    {
        if (sfxChannel != null)
            sfxChannel.OnPlaySound += PlaySFX;

        if (musicChannel != null)
            musicChannel.OnPlayMusic += PlayMusic;
    }

    private void OnDisable()
    {
        if (sfxChannel != null)
            sfxChannel.OnPlaySound -= PlaySFX;

        if (musicChannel != null)
            musicChannel.OnPlayMusic -= PlayMusic;
    }

    //  SFX (3D)
    private void PlaySFX(SoundDefinition sound, Vector3 position)
    {
        if (sound == null) return;

        var instance = RuntimeManager.CreateInstance(sound.eventReference);

        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.setVolume(sound.volume);

        // Optional parameter
        if (!string.IsNullOrEmpty(sound.parameterName))
        {
            instance.setParameterByName(sound.parameterName, sound.parameterValue);
        }

        instance.start();
        instance.release(); // Important!
    }

    //  MUSIC (2D)
    private void PlayMusic(SoundDefinition sound)
    {
        if (sound == null) return;

        // Stop previous music
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }

        musicInstance = RuntimeManager.CreateInstance(sound.eventReference);
        musicInstance.setVolume(sound.volume);
        musicInstance.start();
    }
}
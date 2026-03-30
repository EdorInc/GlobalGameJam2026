using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AudioManager : MonoBehaviour
{
    [Header("Channels")]
    [SerializeField] private AudioEventChannel sfxChannel;
    [SerializeField] private AudioEventChannel musicChannel;

    [Header("Sound Library")]
    [SerializeField] private SoundLibrary soundLibrary;

    private EventInstance musicInstance;

    private Dictionary<int, Dictionary<SoundDefinition, EventInstance>> dynamicSFX = new Dictionary<int, Dictionary<SoundDefinition, EventInstance>>();

    private void Awake()
    {
        AudioSystem.Initialize(sfxChannel, musicChannel, soundLibrary, this);
    }
    private void OnEnable()
    {
        if (sfxChannel != null)
        {
            sfxChannel.OnPlaySound += PlaySFX;
            sfxChannel.OnPlayDynamicSound += PlayDynamicSFX;
            sfxChannel.OnStopDynamicSound += StopDynamicSFX;
        }

        if (musicChannel != null)
            musicChannel.OnPlayMusic += PlayMusic;
    }

    private void OnDisable()
    {
        if (sfxChannel != null)
        {
            sfxChannel.OnPlaySound -= PlaySFX;
            sfxChannel.OnPlayDynamicSound -= PlayDynamicSFX;
            sfxChannel.OnStopDynamicSound -= StopDynamicSFX;
        }

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

    private void PlayDynamicSFX(SoundDefinition sound, Vector3 position, int playerId)
    {
        if (sound == null) return;

        var instance = RuntimeManager.CreateInstance(sound.eventReference);

        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.setVolume(sound.volume);

        instance.start();

        if (!dynamicSFX.ContainsKey(playerId))
        {
            dynamicSFX.Add(playerId, new Dictionary<SoundDefinition, EventInstance>());
        }
        dynamicSFX[playerId][sound] = instance;
    }

    private void StopDynamicSFX(SoundDefinition sound, int playerId)
    {
        if (sound == null) return;
        if (dynamicSFX.TryGetValue(playerId, out var playerSounds) && playerSounds.TryGetValue(sound, out var instance) && instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            playerSounds.Remove(sound);
        }
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

    public void UpdateSoundDefinitionParameters(SoundDefinition sound, string parameterName, object parameterValue)
    {
        if (sound == null || string.IsNullOrEmpty(parameterName) || parameterName == null) return;

        float floatValue;

        switch (parameterValue)
        {
            case float f:
                floatValue = f;
                break;
            case int i:
                floatValue = i;
                break;
            case bool b:
                floatValue = b ? 1f : 0f;
                break;
            default:
                Debug.LogWarning($"Type {parameterValue.GetType()} not supported as FMOD parameter.");
                return;
        }

        foreach(var kvp in dynamicSFX)
        {
            if (kvp.Value.TryGetValue(sound, out var instance) && instance.isValid())
            {
                instance.setParameterByName(parameterName, floatValue);
            }
        }
    }

}
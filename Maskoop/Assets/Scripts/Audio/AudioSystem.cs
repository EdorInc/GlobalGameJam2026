using UnityEngine;

public static class AudioSystem
{
    private static AudioEventChannel sfxChannel;
    private static AudioEventChannel musicChannel;

    public static SoundLibrary SoundLibrary;

    private static AudioManager AudioManager;

    public static void Initialize(AudioEventChannel sfx, AudioEventChannel music, SoundLibrary library, AudioManager audio)
    {
        sfxChannel = sfx;
        musicChannel = music;
        SoundLibrary = library;
        AudioManager = audio;
    }

    public static void PlaySFX(SoundDefinition sound, Vector3 position)
    {
        sfxChannel?.RaiseSound(sound, position);
    }

    public static void PlayDynamicSFX(SoundDefinition sound, Vector3 position, int playerId)
    {
        sfxChannel?.RaiseDynamicSound(sound, position, playerId);
    }

    public static void StopDynamicSFX(SoundDefinition sound, int playerId)
    {
        sfxChannel?.StopDynamicSound(sound, playerId);
    }

    public static void PlayMusic(SoundDefinition sound)
    {
        musicChannel?.RaiseMusic(sound);
    }

    public static void UpdateSFXParameter(SoundDefinition sound, string parameterName, object parameterValue)
    {
        AudioManager?.UpdateSoundDefinitionParameters(sound, parameterName, parameterValue);
    }

}
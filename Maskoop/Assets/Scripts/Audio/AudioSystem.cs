using UnityEngine;

public static class AudioSystem
{
    private static AudioEventChannel sfxChannel;
    private static AudioEventChannel musicChannel;

    public static SoundLibrary SoundLibrary;

    public static void Initialize(AudioEventChannel sfx, AudioEventChannel music, SoundLibrary library)
    {
        sfxChannel = sfx;
        musicChannel = music;
        SoundLibrary = library;
    }

    public static void PlaySFX(SoundDefinition sound, Vector3 position)
    {
        sfxChannel?.RaiseSound(sound, position);
    }

    public static void PlayMusic(SoundDefinition sound)
    {
        musicChannel?.RaiseMusic(sound);
    }
}
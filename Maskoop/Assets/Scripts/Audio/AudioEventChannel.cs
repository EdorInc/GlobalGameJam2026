using System;
using UnityEngine;


/// <summary>
/// Represents a channel for audio events. Raises OnPlaySound event when a sound should be played. 
/// The event carries the SoundDefinition and the position where the sound should be played.
/// </summary>
[CreateAssetMenu(menuName = "Audio/Audio Event Channel")]
public class AudioEventChannel : ScriptableObject
{
    public Action<SoundDefinition, Vector3> OnPlaySound;
    public Action<SoundDefinition, Vector3, int> OnPlayDynamicSound;
    public Action<SoundDefinition, int> OnStopDynamicSound;
    public Action<SoundDefinition> OnPlayMusic;

    public void RaiseSound(SoundDefinition sound, Vector3 position)
    {
        OnPlaySound?.Invoke(sound, position);
    }

    public void RaiseDynamicSound(SoundDefinition sound, Vector3 position, int playerId)
    {
        OnPlayDynamicSound?.Invoke(sound, position, playerId);
    }

    public void StopDynamicSound(SoundDefinition sound, int playerId)
    {
        OnStopDynamicSound?.Invoke(sound, playerId);
    }

    public void RaiseMusic(SoundDefinition sound)
    {
        OnPlayMusic?.Invoke(sound);
    }
}

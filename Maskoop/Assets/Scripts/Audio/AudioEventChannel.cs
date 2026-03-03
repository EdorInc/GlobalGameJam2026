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
    public Action<SoundDefinition> OnPlayMusic;

    public void RaiseEvent(SoundDefinition sound, Vector3 position)
    {
        OnPlaySound?.Invoke(sound, position);
        OnPlayMusic?.Invoke(sound);
    }
}

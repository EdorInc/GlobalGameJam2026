using UnityEngine;

/// <summary>
/// Represents a definition of a sound. It contains an array of AudioClips, a volume, a pitch range and a loop flag.
/// </summary>
[CreateAssetMenu(menuName = "Audio/SoundDefinition")]
public class SoundDefinition : ScriptableObject
{
   public AudioClip[] clips;
   [Range(0f, 1f)] public float volume = 1f;
   public Vector2 pitchRange = new Vector2(0.95f,1.05f);
   public bool loop = false;
   [Range(0, 128)] public int priority = 128; //0 is the highest priority, 128 is the lowest
}

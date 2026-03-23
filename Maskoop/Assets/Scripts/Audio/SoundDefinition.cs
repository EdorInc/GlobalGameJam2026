using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Audio/Sound")]
public class SoundDefinition : ScriptableObject
{
    public EventReference eventReference;

    [Header("Optional Overrides")]
    [Range(0f, 1f)] public float volume = 1f;

    [Tooltip("Optional parameter to control variations inside FMOD")]
    public string parameterName;
    public float parameterValue;
}
using System;
using UnityEngine;


[Serializable]
[CreateAssetMenu(fileName = "NewSound", menuName = "Audio/Sound")]
public class SoundAsset : ScriptableObject
{
    public AudioType audioType;
    public FMODUnity.EventReference eventReference;
    public float volume = 1;
}
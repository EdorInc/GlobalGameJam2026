using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    public SoundDefinition footstep;
    public SoundDefinition grab;
    public SoundDefinition chargeThrow;
    public SoundDefinition rockBreak;
    public SoundDefinition throwRelease;
    public SoundDefinition falling;
    public SoundDefinition fallImpact;
    public SoundDefinition hurtSound;
    public SoundDefinition slidingDoorOpen;
    public SoundDefinition slidingDoorClose;
    public SoundDefinition slidingDoorClack;
    public SoundDefinition slidingDoorEngage;
    public SoundDefinition windCurrent;
    public SoundDefinition pressurePlateOn;
    public SoundDefinition pressurePlateOff;
}
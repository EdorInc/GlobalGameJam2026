using UnityEngine;

[CreateAssetMenu(fileName = "NewVFX", menuName = "VFX/VFXAsset")]
public class VFXAsset : ScriptableObject
{
    public VFXType vfxType;
    public GameObject prefab;
    public float lifetime = 2f;
}
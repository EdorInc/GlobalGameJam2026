using System.Collections.Generic;
using UnityEngine;



public enum VFXType 
{
    Explosion,
    Sparkle,
    Smoke
}

public class VFXManager : MonoBehaviour
{

    private static VFXManager _instance;
    public static VFXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("VFXManager");
                _instance = go.AddComponent<VFXManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [SerializeField] private List<VFXAsset> vfxAssets;

    private Dictionary<VFXType, VFXAsset> vfxDictionary;


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        BuildDictionary();
    }

    private void BuildDictionary()
    {
        vfxDictionary = new Dictionary<VFXType, VFXAsset>();

        foreach (var vfx in vfxAssets)
        {
            if (!vfxDictionary.ContainsKey(vfx.vfxType))
                vfxDictionary.Add(vfx.vfxType, vfx);
        }
    }


    public void PlayVFX(VFXType type, Vector3 position, Quaternion rotation = default)
    {
        if (!vfxDictionary.TryGetValue(type, out VFXAsset vfx))
        {
            Debug.LogWarning($"VFX {type} not found");
            return;
        }

        GameObject instance = Instantiate(
            vfx.prefab,
            position,
            rotation == default ? Quaternion.identity : rotation
        );

        Destroy(instance, vfx.lifetime);
    }


    public void PlayVFX(VFXType type, Transform parent)
    {
        if (!vfxDictionary.TryGetValue(type, out VFXAsset vfx))
        {
            Debug.LogWarning($"VFX {type} not found");
            return;
        }

        GameObject instance = Instantiate(
            vfx.prefab,
            parent.position,
            parent.rotation,
            parent
        );

        Destroy(instance, vfx.lifetime);
    }
}

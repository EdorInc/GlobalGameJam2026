using FMOD;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class MaskManager : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private Mask currentMask = Mask.Unmasked;

    private CharacterController controller;

    private LayerMask defaultInclude;
    private LayerMask defaultExclude;

    private AudioManager audioManager;

    // Map each view mode to one or more layers
    private readonly Dictionary<Mask, string[]> excludedCollisionLayers = new()
    {
        { Mask.Unmasked,    new[] { "BlueMask", "GreenMask", "RedMask" }    },
        { Mask.Red,         new[] { "BlueMask", "GreenMask" }               },
        { Mask.Blue,        new[] { "GreenMask", "RedMask" }                },
        { Mask.Green,       new[] { "BlueMask", "RedMask" }                 }
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            UnityEngine.Debug.LogError("No CharacterController found on MaskManager's GameObject.");
            return;
        }

        audioManager = AudioManager.Instance;

        if(audioManager == null)
        {
            UnityEngine.Debug.LogError("No AudioManager found.");
            return;
        }

        defaultInclude = controller.includeLayers;
        defaultExclude = controller.excludeLayers;

        ApplyMask(currentMask);
    }

    void OnValidate()
    {
        if (controller != null)
            ApplyMask(currentMask);
    }

    public void ApplyMask(Mask mask)
    {
        if (controller == null)
        {
            UnityEngine.Debug.LogWarning("No CharacterController found on MaskManager's GameObject.");
            return;
        }

        controller.includeLayers = defaultInclude;
        controller.excludeLayers = defaultExclude;

        if (!currentMask.Equals(Mask.Blue))
        {
            if(mask.Equals(Mask.Blue))
            {
                MovingObject.canMove = true;
            }
        }
        else
        {
            MovingObject.canMove = false;
        }

        // if (currentMask.Equals(Mask.Blue))
        // {
        //     MovingObject.canMove = true;
        // }
        // else
        // {
        //     MovingObject.canMove = false;
        // }

        currentMask = mask;

        if (!excludedCollisionLayers.TryGetValue(mask, out var layers))
            return;

        LayerMask exclude = controller.excludeLayers;

        foreach (var layerName in layers)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                UnityEngine.Debug.LogWarning($"Layer '{layerName}' does not exist.");
                continue;
            }

            exclude |= 1 << layer;
        }

        controller.excludeLayers = exclude;


        if (audioManager != null)
        {
            ATTRIBUTES_3D attr = new ATTRIBUTES_3D();

            attr.position = RuntimeUtils.ToFMODVector(transform.position);
            attr.forward = RuntimeUtils.ToFMODVector(transform.forward);
            attr.up = RuntimeUtils.ToFMODVector(transform.up);

            AudioManager.Instance.PlaySFX(AudioType.Equip, attr);

            if (this.gameObject == ReferenceManager.Instance.GetPlayerOne())
            {
                audioManager.UpdateMaskParameter(currentMask, ReferenceManager.Instance.GetPlayerTwoMask().GetCurrentMask());
            }
            else
            {
                audioManager.UpdateMaskParameter(ReferenceManager.Instance.GetPlayerOneMask().GetCurrentMask(), currentMask);
            }
        }

    }

    public Mask GetCurrentMask() => currentMask;
}

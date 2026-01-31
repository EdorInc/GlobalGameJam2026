using System.Collections.Generic;
using UnityEngine;

public class MaskManager : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private Mask currentMask = Mask.Unmasked;

    private CharacterController controller;

    private LayerMask defaultInclude;
    private LayerMask defaultExclude;

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
            return;

        defaultInclude = controller.includeLayers;
        defaultExclude = controller.excludeLayers;

        ApplyMask(currentMask);
    }

    void OnValidate()
    {
        ApplyMask(currentMask);
    }

    private void ApplyMask(Mask mask)
    {
        if (controller == null)
            return;

        controller.includeLayers = defaultInclude;
        controller.excludeLayers = defaultExclude;

        currentMask = mask;

        if (!excludedCollisionLayers.TryGetValue(mask, out var layers))
            return;

        LayerMask exclude = controller.excludeLayers;

        foreach (var layerName in layers)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                Debug.LogWarning($"Layer '{layerName}' does not exist.");
                continue;
            }

            exclude |= 1 << layer;
        }

        controller.excludeLayers = exclude;
    }

    public Mask GetCurrentMask() => currentMask;
}

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CullingMaskController : MonoBehaviour
{
    private Camera cam;
    private int defaultCullingMask;

    [Header("State")]
    [SerializeField] private Mask currentView = Mask.Unmasked;

    // Map each view mode to one or more layers
    private readonly Dictionary<Mask, string[]> viewLayers = new()
    {
        { Mask.Red,   new[] { "RedMask", "NoMask" } },
        { Mask.Blue,  new[] { "BlueMask", "NoMask" } },
        { Mask.Green, new[] { "GreenMask" } }
    };

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        if (cam == null) return;

        // Save scene-configured mask as default
        defaultCullingMask = cam.cullingMask;
        ApplyView(Mask.Unmasked);
    }

    void OnValidate()
    {
        ApplyView(currentView);
    }

    public void ApplyView(Mask mode)
    {
        if (cam == null) return;

        // Always reset first
        cam.cullingMask = defaultCullingMask;
        currentView = mode;

        if (mode == Mask.Unmasked)
            return;

        if (!viewLayers.TryGetValue(mode, out var layers))
            return;

        foreach (var layerName in layers)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                Debug.LogWarning($"Layer '{layerName}' does not exist.");
                continue;
            }

            cam.cullingMask |= 1 << layer;
        }
    }
}
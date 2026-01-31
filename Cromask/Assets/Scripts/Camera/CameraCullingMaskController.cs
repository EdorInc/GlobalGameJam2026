using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraCullingMaskController : MonoBehaviour
{
    private Camera cam;
    private int defaultCullingMask;

    public enum ViewMode
    {
        Default,
        Red,
        Blue,
        Green
    }

    [Header("State")]
    [SerializeField] private ViewMode currentView = ViewMode.Default;

    // Map each view mode to one or more layers
    private readonly Dictionary<ViewMode, string[]> viewLayers = new()
    {
        { ViewMode.Red,   new[] { "RedMask", "NoMask" } },
        { ViewMode.Blue,  new[] { "BlueMask", "NoMask" } },
        { ViewMode.Green, new[] { "GreenMask" } }
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
        ApplyView(ViewMode.Default);
    }

    void OnValidate()
    {
        ApplyView(currentView);
    }

    public void ApplyView(ViewMode mode)
    {
        if (cam == null) return;

        // Always reset first
        cam.cullingMask = defaultCullingMask;
        currentView = mode;

        if (mode == ViewMode.Default)
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

    // Convenience methods (nice for UI buttons)
    public void SetDefault() => ApplyView(ViewMode.Default);
    public void SetRed() => ApplyView(ViewMode.Red);
    public void SetBlue() => ApplyView(ViewMode.Blue);
    public void SetGreen() => ApplyView(ViewMode.Green);

    /// <summary>
    /// Returns the currently active view mode
    /// </summary>
    public ViewMode GetCurrentView()
    {
        return currentView;
    }

    /// <summary>
    /// Useful for UI text/debug display
    /// </summary>
    public string GetCurrentViewName()
    {
        return currentView.ToString();
    }
}
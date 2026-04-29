using UnityEngine;

/// <summary>
/// Describes a block type that can be placed into the map editor.
/// </summary>
[CreateAssetMenu(fileName = "BlockDefinition", menuName = "Game Data/Block Definition", order = 1)]
public class BlockDefinitionSO : ScriptableObject
{
    /// <summary>
    /// Unique identifier used to reference this block in map data.
    /// </summary>
    [Tooltip("Unique identifier used to reference this block in map data.")]
    [SerializeField] private string m_blockId;

    /// <summary>
    /// Human-readable name shown in the editor UI.
    /// </summary>
    [Tooltip("Human-readable name shown in the editor UI.")]
    [SerializeField] private string m_displayName;

    /// <summary>
    /// Small preview image used to visually represent the block in menus.
    /// </summary>
    [Tooltip("Small preview image used to visually represent the block in menus.")]
    [SerializeField] private Texture2D m_previewTexture;

    /// <summary>
    /// Prefab instantiated when this block is placed in the world.
    /// </summary>
    [Tooltip("Prefab instantiated when this block is placed in the world.")]
    [SerializeField] private GameObject m_prefab;

    /// <summary>
    /// Gets the unique identifier for this block.
    /// </summary>
    public string BlockId => m_blockId;

    /// <summary>
    /// Gets the display name shown to users in the editor.
    /// </summary>
    public string DisplayName => m_displayName;

    /// <summary>
    /// Gets the preview texture used by the editor UI.
    /// </summary>
    public Texture2D PreviewTexture => m_previewTexture;

    /// <summary>
    /// Gets the prefab that represents this block in the scene.
    /// </summary>
    public GameObject Prefab => m_prefab;
}
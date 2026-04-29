using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the serialized data for a 3D map grid used by the tilemap editor.
/// </summary>
[CreateAssetMenu(fileName = "MapData", menuName = "Game Data/Map", order = 0)]
public class MapDataSO : ScriptableObject
{
    [SerializeField] private int m_width = 10;
    [SerializeField] private int m_height = 4;
    [SerializeField] private int m_depth = 10;
    [SerializeField] private List<MapCellData> m_cells = new();

    /// <summary>
    /// Gets the number of cells along the X axis.
    /// </summary>
    public int Width => m_width;

    /// <summary>
    /// Gets the number of cells along the Y axis.
    /// </summary>
    public int Height => m_height;

    /// <summary>
    /// Gets the number of cells along the Z axis.
    /// </summary>
    public int Depth => m_depth;

    /// <summary>
    /// Allocates the map dimensions and creates one empty cell for every position in the grid.
    /// </summary>
    public void Initialize(int width, int height, int depth)
    {
        m_width = width;
        m_height = height;
        m_depth = depth;

        // Pre-size the list so each grid position has a matching cell entry.
        m_cells = new List<MapCellData>(width * height * depth);

        // Fill the list with default cell data so every coordinate can be indexed safely.
        for (int i = 0; i < width * height * depth; i++)
            m_cells.Add(new MapCellData());
    }

    /// <summary>
    /// Checks whether a coordinate is inside the current map bounds.
    /// </summary>
    /// <returns><c>true</c> if the coordinate is valid; otherwise, <c>false</c>.</returns>
    public bool IsValidCoord(int x, int y, int z) => x >= 0 && x < m_width && y >= 0 && y < m_height && z >= 0 && z < m_depth;
    
    /// <summary>
    /// Gets the cell stored at the specified coordinate.
    /// </summary>
    /// <returns>The cell data at the requested position.</returns>
    public MapCellData GetCell(int x, int y, int z) => m_cells[IndexOf(x, y, z)];

    /// <summary>
    /// Stores cell data at the specified coordinate.
    /// </summary>
    /// <param name="data">The cell data to store.</param>
    public void SetCell(int x, int y, int z, MapCellData data) => m_cells[IndexOf(x, y, z)] = data;


    /// <summary>
    /// Converts a 3D coordinate into the matching index inside the flat cell list.
    /// </summary>
    /// <returns>The zero-based index into <see cref="m_cells"/>.</returns>
    private int IndexOf(int x, int y, int z) => x + m_width * (y + m_height * z);
}

/// <summary>
/// Represents the data stored for a single map cell.
/// </summary>
[Serializable]
public struct MapCellData
{
    /// <summary>
    /// The identifier of the block placed in this cell.
    /// </summary>
    public string BlockId;

    /// <summary>
    /// Indicates whether the cell is empty and contains no block.
    /// </summary>
    public bool IsEmpty;
}
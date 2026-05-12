using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Game Data/Map", order = 0)]
public class MapDataSO : ScriptableObject
{
    [SerializeField] private int m_width = 10;
    [SerializeField] private int m_height = 4;
    [SerializeField] private int m_depth = 10;
    [SerializeField] private List<MapCellData> m_cells = new();

    public int Width => m_width;
    public int Height => m_height;
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
    /// Resizes the map boundaries. Will expand outwards from the center of currently placed blocks,
    /// and will try to shift them inward when shrinking to prevent eating blocks if possible.
    /// </summary>
    public void Resize(int newWidth, int newHeight, int newDepth)
    {
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        int minZ = int.MaxValue, maxZ = int.MinValue;
        bool hasBlocks = false;

        // Gather bounding box of existing blocks
        for (int z = 0; z < m_depth; z++)
        {
            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    if (!GetCell(x, y, z).IsEmpty)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                        if (z < minZ) minZ = z;
                        if (z > maxZ) maxZ = z;
                        hasBlocks = true;
                    }
                }
            }
        }

        // Calculate centered offsets
        int offsetX = GetOffset(m_width, newWidth, minX, maxX, hasBlocks);
        int offsetY = GetOffset(m_height, newHeight, minY, maxY, hasBlocks);
        int offsetZ = GetOffset(m_depth, newDepth, minZ, maxZ, hasBlocks);

        // Populate new array
        var newCells = new List<MapCellData>(newWidth * newHeight * newDepth);
        for (int i = 0; i < newWidth * newHeight * newDepth; i++)
            newCells.Add(new MapCellData { IsEmpty = true });

        // Translate existing cells over to the new array using offsets
        for (int z = 0; z < newDepth; z++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int oldX = x - offsetX;
                    int oldY = y - offsetY;
                    int oldZ = z - offsetZ;

                    if (IsValidCoord(oldX, oldY, oldZ))
                    {
                        int newIndex = x + newWidth * (y + newHeight * z);
                        newCells[newIndex] = GetCell(oldX, oldY, oldZ);
                    }
                }
            }
        }

        // Apply
        m_width = newWidth;
        m_height = newHeight;
        m_depth = newDepth;
        m_cells = newCells;
    }

    private int GetOffset(int oldSize, int newSize, int min, int max, bool hasBlocks)
    {
        // If empty, perfectly center physical size
        if (!hasBlocks)
        {
            return (newSize - oldSize) / 2;
        }

        int oldMid = (min + max) / 2;
        int newMid = newSize / 2;
        int offset = newMid - oldMid;

        // Clamp the offset so we don't accidentally push blocks out of bounds if there's enough room available
        if (offset < -min)
        {
            offset = -min;
        }
        if (offset > newSize - 1 - max)
        {
            offset = newSize - 1 - max;
        }

        return offset;
    }

    public bool IsValidCoord(int x, int y, int z) => x >= 0 && x < m_width && y >= 0 && y < m_height && z >= 0 && z < m_depth;
    
    public MapCellData GetCell(int x, int y, int z) => m_cells[IndexOf(x, y, z)];

    public void SetCell(int x, int y, int z, MapCellData data) => m_cells[IndexOf(x, y, z)] = data;

    /// <summary>
    /// Converts a 3D coordinate into the matching index inside the flat cell list.
    /// </summary>
    /// <returns>The zero-based index into <see cref="m_cells"/>.</returns>
    private int IndexOf(int x, int y, int z) => x + m_width * (y + m_height * z);
}

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
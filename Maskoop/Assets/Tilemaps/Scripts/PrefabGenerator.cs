using UnityEngine;
using UnityEngine.Tilemaps;

public class PrefabGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] Grid mapGrid;
    [SerializeField] string mapName;

    [Header("Prefabs Settings")]
    [SerializeField] GameObject floorPrefab;
    [SerializeField] GameObject wallPrefab;

    [ContextMenu("Save")]
    void Save()
    {
        Tilemap[] levels = mapGrid.GetComponentsInChildren<Tilemap>();

        for (int i = 0; i < levels.Length; i++)
        {
            Tilemap level = levels[i];

            // Get all floor blocks in the tilemap
            // Get all the transforms of the floor blocks in the tilemap
            // Add a floor prefab with the same position and rotation as the floor block

            // Make the floor bloock children of a empty called "Floor"

            // Get all wall blocks in the tilemap
            // Get all the transforms of the wall blocks in the tilemap
            // Gather all transforms lined on the Z axis with more than 1 block in between them
            // Add a wall prefab within the center of the gathered transforms and modify the scale of the wall prefab
            // Delete the gathered transforms
            // Gather the transforms lined on the X axis with more than 1 block in between them
            // Add a wall prefab within the center of the gathered transforms and modify the scale of the wall prefab
            // Delete the gathered transforms
            // Add wall prefabs on the remaining transforms

            // Make the wall blocks children of a empty called "Wall"

            // Make the floor and wall empty children of a empty called "Level"
        }

        // string path = $"Assets/Tilemaps/Prefabs/Level_{mapName}.prefab";
        // PrefabUtility.SaveAsPrefabAsset(level.gameObject, path);
    }
}

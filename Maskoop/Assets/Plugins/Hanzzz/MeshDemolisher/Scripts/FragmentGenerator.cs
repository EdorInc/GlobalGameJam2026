using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hanzzz.MeshDemolisher
{

public class FragmentGenerator : MonoBehaviour
{
    [SerializeField] private GameObject targetGameObject;
    [SerializeField] private Transform breakPointsParent;
    [SerializeField] private Material interiorMaterial;

    [SerializeField] private Key demolishKey = Key.Space;

    [SerializeField] [Range(0f,1f)] private float resultScale;
    [SerializeField] private Transform resultParent;

    private static MeshDemolisher meshDemolisher = new MeshDemolisher();

    private void Update()
    {
          if (Keyboard.current[demolishKey].wasPressedThisFrame)
          {
              if (targetGameObject.activeSelf)
                  Demolish();
              else
                  Reset();
          }
     }

    [ContextMenu("Validate")]
    public void Validate()
    {
        List<Transform> breakPoints = Enumerable.Range(0,breakPointsParent.childCount).Select(x=>breakPointsParent.GetChild(x)).ToList();

        bool res = meshDemolisher.VerifyDemolishInput(targetGameObject, breakPoints);
        if(res)
        {
            Debug.Log("Demolish input looks good.");
        }
    }

    [ContextMenu("Demolish")]
    public void Demolish()
    {
        Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));
        List<Transform> breakPoints = Enumerable.Range(0,breakPointsParent.childCount).Select(x=>breakPointsParent.GetChild(x)).ToList();

        List<GameObject> res = meshDemolisher.Demolish(targetGameObject, breakPoints, interiorMaterial);

        // res.ForEach(x=>x.transform.SetParent(resultParent, true));
        // Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>x.localScale=resultScale*Vector3.one);

        int index = 0;

        res.ForEach(piece =>
        {
            piece.transform.SetParent(resultParent, true);
            piece.transform.localScale = resultScale * Vector3.one;

            // Save the mesh as an asset to avoid modifying the original mesh when adding a MeshCollider
            MeshFilter mf = piece.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
#if UNITY_EDITOR
                string baseFolder = "Assets/Generated";
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
                string folderPath = $"{baseFolder}/{timestamp}";

                // Check if folder exists, if not create it
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    // Create the base folder first if it doesn't exist
                    if (!AssetDatabase.IsValidFolder(baseFolder))
                    {
                        AssetDatabase.CreateFolder("Assets", "Generated");
                    }

                    // Create the timestamped subfolder
                    string newFolderName = timestamp;
                    AssetDatabase.CreateFolder(baseFolder, newFolderName);
                }

                Debug.Log($"Folder created or already exists: {folderPath}");

                string meshPath = $"{folderPath}/{piece.name}_{index}.asset";

                Mesh meshCopy = Instantiate(mf.sharedMesh);
                AssetDatabase.CreateAsset(meshCopy, meshPath);
                AssetDatabase.SaveAssets();

                mf.sharedMesh = meshCopy;
#endif
            }

            MeshCollider meshCollider = piece.GetComponent<MeshCollider>();
            if (meshCollider == null)
                meshCollider = piece.AddComponent<MeshCollider>();

            meshCollider.convex = true;

            Rigidbody rb = piece.GetComponent<Rigidbody>();
            if (rb == null)
                rb = piece.AddComponent<Rigidbody>();

            rb.mass = 1f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            index++;
        });

        targetGameObject.SetActive(false);
    }

    [ContextMenu("Reset")]
    public void Reset()
    {
        Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));

        targetGameObject.SetActive(true);
    }

    public void OnValidate()
    {
        Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>x.localScale=resultScale*Vector3.one);
    }
}

}

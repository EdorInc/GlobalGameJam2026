using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TransformList
{
    public List<Transform> transforms;
}


public class NavMeshManager : MonoBehaviour
{


    [SerializeField]
    public List<TransformList> transformList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector3 GetRandomPointInMap()
    {
        int i = Random.Range(0,transformList.Count);
        int j = Random.Range(0, transformList[i].transforms.Count);

        return transformList[i].transforms[j].position;
    }
}

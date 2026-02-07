using UnityEngine;

public class CarouselController : MonoBehaviour
{
    [Header("Rotation")]
    public float speed = 30f;

    [Header("Layout")]
    public float radius = 2f;
    public Transform center;

    void Start()
    {
        PositionChildren();
    }

    void Update()
    {
        if (center != null)
        {
            transform.RotateAround(center.position, Vector3.up, speed * Time.deltaTime);
        }
        else
        {
            transform.Rotate(0f, speed * Time.deltaTime, 0f, Space.World);
        }
    }

    void OnValidate()
    {
        PositionChildren();
    }

    void PositionChildren()
    {
        if (center == null) return;

        int count = 0;

        foreach (Transform child in transform)
        {
            if (child == center) continue;
            count++;
        }

        if (count == 0) return;

        float angleStep = 360f / count;
        int index = 0;

        foreach (Transform child in transform)
        {
            if (child == center) continue;

            float angle = index * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;

            child.position = center.position + new Vector3(x, 0f, z);
            child.rotation = Quaternion.Euler(0f, index * angleStep, 0f);

            index++;
        }
    }
}

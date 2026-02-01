using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
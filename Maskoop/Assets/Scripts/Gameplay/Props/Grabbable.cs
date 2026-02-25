using UnityEngine;

public class Grabbable : MonoBehaviour
{
    [Header("Optional Settings")]
    public Vector3 holdOffset = Vector3.zero;   // local position when held
    public Quaternion holdRotation = Quaternion.identity; // local rotation when held
    [Header("Throw force Settings")]
    public Vector2 maxThrowForce = new Vector2(8,8);
    public Vector2 minThrowForce = new Vector2(0, 0);
    public float forceGrowRate = 2;
}
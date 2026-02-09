using UnityEngine;

public class Grabbable : MonoBehaviour
{
    [Header("Optional Settings")]
    public Vector3 holdOffset = Vector3.zero;   // local position when held
    public Quaternion holdRotation = Quaternion.identity; // local rotation when held
}
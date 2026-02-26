using UnityEngine;

public class Equipable : MonoBehaviour
{
    [Header("Optional Settings")]
    public float equipOffset = 0.2f; // local position when equiped
    public float equipVerticalOffset = 0.2f;
    public Quaternion equipOffsetRotation = Quaternion.identity; // local rotation when equiped

    public void Equip(Transform equipPosition)
    {
    }
    public void UnEquip()
    {
    }
}

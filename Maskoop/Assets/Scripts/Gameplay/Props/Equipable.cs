using UnityEngine;

public class Equipable : MonoBehaviour
{
    [Header("Optional Settings")]
    public Vector3 equipOffset = Vector3.zero; // local position when equiped

    public void Equip()
    {
        transform.position += equipOffset;
        transform.localScale -= new Vector3(0.2f, 0.2f, 0.2f);
    }
    public void UnEquip()
    {
        transform.position -= equipOffset;
        transform.localScale += new Vector3(0.2f, 0.2f, 0.2f);
    }
}

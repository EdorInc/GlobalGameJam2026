using UnityEngine;

public class EquipableObject : MonoBehaviour
{
    [SerializeField] private Mask maskType = Mask.Unmasked;

    public Mask Equip()
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent <Rigidbody>().isKinematic = true;
        Debug.Log("Object equipped");

        return maskType;
    }

    public void UnEquip()
    {
        GetComponent<Collider>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        Debug.Log("Object unequipped");
    }
}

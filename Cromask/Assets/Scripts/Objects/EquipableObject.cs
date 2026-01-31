using UnityEngine;

public class EquipableObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Equip()
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent <Rigidbody>().isKinematic = true;
        Debug.Log("Object equipped");
    }

    public void UnEquip()
    {
        GetComponent<Collider>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        Debug.Log("Object unequipped");
    }
}

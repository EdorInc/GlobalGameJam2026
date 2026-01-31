using UnityEngine;

public class EquipableManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private GameObject equipedObject;

    public void Equip(GameObject mask)
    {
        equipedObject = mask;
    }

    public GameObject UnEquip()
    {
        return equipedObject;
    }
}

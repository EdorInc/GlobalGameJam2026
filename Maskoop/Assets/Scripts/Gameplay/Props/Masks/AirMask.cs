using UnityEngine;

public class AirMask : BaseMask
{
    public override void OnUnequip()
    {
        Debug.Log("Me quito la mascara verde");
    }

    public override void UpdateLogic()
    {
        Debug.Log("Update la mascara verde");
    }

    public override void OnEquip(CharacterStateController characterState)
    {
        base.OnEquip(characterState);
        Debug.Log("Tengo la mascara verde");
    }
}

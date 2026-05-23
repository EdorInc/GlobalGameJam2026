using UnityEngine;
[DefaultExecutionOrder(300)]
public class FireMask : BaseMask
{
    public override void FixedUpdateLogic() { }

    public override void OnEquip(CharacterStateController characterState)
    {
        base.OnEquip(characterState);
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
    }

    public override void UpdateLogic() { }
}

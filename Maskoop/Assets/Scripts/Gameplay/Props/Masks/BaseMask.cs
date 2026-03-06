using System;
using UnityEngine;

public abstract class BaseMask : MonoBehaviour
{
    protected CharacterStateController characterState;

    public abstract void UpdateLogic();

    public abstract void FixedUpdateLogic();

    public abstract void OnUnequip();

    public virtual void OnEquip(CharacterStateController characterState)
    {
        this.characterState = characterState;
    }
}

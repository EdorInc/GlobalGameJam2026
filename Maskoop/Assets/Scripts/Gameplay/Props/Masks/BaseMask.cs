using System;
using UnityEngine;

public abstract class BaseMask : MonoBehaviour
{
    public abstract void UpdateLogic();

    public abstract void OnUnequip();

    public abstract void OnEquip();
}

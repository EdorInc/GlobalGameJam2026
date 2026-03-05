using System;
using UnityEngine;

public abstract class BaseMask : MonoBehaviour
{
    public abstract void updateLogic();

    public abstract void OnUnequip();

    public abstract void OnEquip();
}

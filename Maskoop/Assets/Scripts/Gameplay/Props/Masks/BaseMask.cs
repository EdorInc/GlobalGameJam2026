using System;
using UnityEngine;

public abstract class BaseMask : MonoBehaviour
{
    protected CharacterStateController characterState;
    private Respawn respawnComponent;

    private void Start()
    {
        respawnComponent = GetComponent<Respawn>();
        respawnComponent.respawnPosition = transform.position;
    }

    public abstract void UpdateLogic();

    public abstract void FixedUpdateLogic();

    public virtual void OnEquip(CharacterStateController characterState)
    {
        this.characterState = characterState;
    }

    public abstract void OnUnequip();

    public virtual void Respawn()
    {
        respawnComponent.RespawnFunction();
    }
}

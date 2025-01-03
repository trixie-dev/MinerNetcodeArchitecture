using Unity.Netcode;
using UnityEngine;

public abstract class CharacterEntity : Entity
{
    protected BackpackComponent backpack;

    protected override void Awake()
    {
        base.Awake();
        backpack = GetComponent<BackpackComponent>();
    }

    public virtual bool AddResource(float amount)
    {
        return backpack?.AddResource(amount) ?? false;
    }

    public virtual void TakeBackpackDamage(float damage)
    {
        backpack?.TakeDamage(damage);
    }
}
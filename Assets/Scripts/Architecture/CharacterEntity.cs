using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(BackpackComponent))]
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
}
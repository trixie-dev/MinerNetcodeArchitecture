using UnityEngine;

public abstract class State
{
    protected Entity owner;

    public State(Entity owner)
    {
        this.owner = owner;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}
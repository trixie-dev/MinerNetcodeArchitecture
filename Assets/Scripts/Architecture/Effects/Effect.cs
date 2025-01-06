public abstract class Effect
{
    public float Duration { get; protected set; }
    public float RemainingTime { get; protected set; }
    public bool IsActive => RemainingTime > 0;

    protected Entity target;

    public Effect(Entity target, float duration)
    {
        this.target = target;
        Duration = duration;
        RemainingTime = duration;
    }

    public virtual void Apply() { }
    public virtual void Remove() { }

    public virtual void Update(float deltaTime)
    {
        if (!IsActive) return;

        RemainingTime -= deltaTime;
        if (RemainingTime <= 0)
        {
            Remove();
        }
    }
}
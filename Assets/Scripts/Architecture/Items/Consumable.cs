using UnityEngine;

public abstract class Consumable
{
    public float Cooldown { get; protected set; }
    public float LastUseTime { get; protected set; }

    public Consumable(float cooldown = 10f)
    {
        Cooldown = cooldown;
        LastUseTime = -cooldown; // Щоб можна було використати одразу
    }

    public bool CanUse(float currentTime) => currentTime >= LastUseTime + Cooldown;

    public void StartCooldown(float currentTime)
    {
        LastUseTime = currentTime;
    }

    public abstract void Use(Entity user);
}
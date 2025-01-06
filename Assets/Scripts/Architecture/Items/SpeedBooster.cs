using Unity.Netcode;
using UnityEngine;

public class SpeedBooster : Consumable
{
    private readonly float duration;

    public SpeedBooster(float duration = 5f, float cooldown = 10f) : base(cooldown)
    {
        this.duration = duration;
    }

    public override void Use(Entity user)
    {
        DebugUtil.LogGameplay($"SpeedBooster.Use called for {user.name}");

        var effects = user.GetComponent<EffectsComponent>();
        if (effects != null)
        {
            DebugUtil.LogGameplay($"Found EffectsComponent, applying speed boost");
            effects.AddEffectServerRpc(EffectType.SpeedBoost, duration);
        }
        else
        {
            DebugUtil.LogGameplay("No EffectsComponent found!", LogType.Error);
        }
    }
}
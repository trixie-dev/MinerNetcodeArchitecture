using UnityEngine;

public class AoEStunner : Consumable
{
    private readonly float radius;
    private readonly float duration;

    public AoEStunner(float radius = 5f, float duration = 5f, float cooldown = 10f) : base(cooldown)
    {
        this.radius = radius;
        this.duration = duration;
    }

    public override void Use(Entity user)
    {
        DebugUtil.LogGameplay($"AoEStunner.Use called by {user.name}");

        var colliders = Physics2D.OverlapCircleAll(user.Position, radius);
        int affectedTargets = 0;

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<Entity>(out var entity))
            {
                if (entity == user) continue;

                var effects = entity.GetComponent<EffectsComponent>();
                if (effects != null)
                {
                    effects.AddEffectServerRpc(EffectType.Stun, duration);
                    affectedTargets++;

                    // Логуємо для відлагодження
                    string targetType = entity.GetType().Name;
                    ulong targetId = entity.NetworkObjectId;
                    DebugUtil.LogGameplay($"Attempting to stun {targetType} (ID: {targetId})");
                }
                else
                {
                    DebugUtil.LogGameplay($"No EffectsComponent found on {entity.name}!", LogType.Warning);
                }
            }
        }

        DebugUtil.LogGameplay($"AoEStunner affected {affectedTargets} targets");
    }
}
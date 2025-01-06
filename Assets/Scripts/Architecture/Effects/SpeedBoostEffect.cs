using UnityEngine;

public class SpeedBoostEffect : Effect
{
    private readonly float speedMultiplier;
    private EntityStatsComponent stats;

    public SpeedBoostEffect(Entity target, float duration, float multiplier = 2f)
        : base(target, duration)
    {
        speedMultiplier = multiplier;
        stats = target.GetComponent<EntityStatsComponent>();

        if (stats == null)
        {
            DebugUtil.LogGameplay($"No EntityStatsComponent found on {target.name}!", LogType.Error);
        }
    }

    public override void Apply()
    {
        DebugUtil.LogGameplay($"Applying SpeedBoostEffect with multiplier {speedMultiplier}");
        if (stats != null)
        {
            stats.ModifySpeedMultiplier(speedMultiplier);
            DebugUtil.LogGameplay($"Speed boost applied, new speed: {stats.MoveSpeed}");
        }
    }

    public override void Remove()
    {
        DebugUtil.LogGameplay($"Removing SpeedBoostEffect");
        if (stats != null)
        {
            stats.ModifySpeedMultiplier(1f / speedMultiplier);
            DebugUtil.LogGameplay($"Speed boost removed, new speed: {stats.MoveSpeed}");
        }
    }
}
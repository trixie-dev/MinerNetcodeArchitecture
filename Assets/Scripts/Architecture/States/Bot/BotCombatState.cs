using UnityEngine;

public class BotCombatState : State
{
    private BotEntity bot;
    private Entity target;
    private float checkInterval = 0.5f;
    private float nextCheckTime;
    private float combatRange = 10f;
    private float preferredAttackRange = 1.5f;
    private NetworkMovementComponent movement;

    public BotCombatState(BotEntity owner) : base(owner)
    {
        bot = owner;
        movement = owner.GetComponent<NetworkMovementComponent>();
    }

    public override void Enter()
    {
        // Знаходимо найближчого моба або ворожого гравця
        FindTarget();
    }

    public override void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            if (target == null || !IsTargetInRange())
            {
                FindTarget();
                if (target == null)
                {
                    // Якщо цілі немає, повертаємось до збору ресурсів
                    bot.stateMachine.SetState<BotGatheringState>();
                    return;
                }
            }
            nextCheckTime = Time.time + checkInterval;
        }

        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(bot.Position, target.Position);

            if (distanceToTarget <= preferredAttackRange)
            {
                // Атакуємо ціль
                bot.Attack(target);
            }
            else if (distanceToTarget <= combatRange)
            {
                // Наближаємося до цілі
                movement.MoveToPosition(target.Position);
            }
            else
            {
                // Ціль занадто далеко, повертаємось до збору ресурсів
                bot.stateMachine.SetState<BotGatheringState>();
            }
        }
    }

    private void FindTarget()
    {
        // Спочатку шукаємо мобів
        var nearestMob = ObjectManager.Instance.GetNearestMob(bot.Position);
        if (nearestMob != null && Vector2.Distance(bot.Position, nearestMob.Position) <= combatRange)
        {
            target = nearestMob;
            return;
        }

        // Якщо мобів немає, можемо шукати ворожих гравців
        // (якщо в грі є PvP механіки)
    }

    private bool IsTargetInRange()
    {
        return target != null &&
               Vector2.Distance(bot.Position, target.Position) <= combatRange;
    }
}
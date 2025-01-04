using UnityEngine;

public class MobAttackState : State
{
    private MobEntity mob;
    private CharacterEntity target;
    private float attackRange;
    private float checkTargetInterval = 0.5f;
    private float nextCheckTime;

    public MobAttackState(MobEntity owner) : base(owner)
    {
        mob = owner;
        attackRange = 2f; // Можна зробити налаштовуваним
    }

    public override void Enter()
    {
        // Знаходимо ціль при вході в стан
        target = ObjectManager.Instance.GetNearestPlayer(mob.Position);
    }

    public override void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            // Періодично перевіряємо ціль
            if (target == null || !IsTargetInRange())
            {
                mob.stateMachine.SetState<MobChaseState>();
                return;
            }
            nextCheckTime = Time.time + checkTargetInterval;
        }

        if (target != null)
        {
            // Повертаємося до цілі
            Vector2 direction = (target.Position - mob.Position).normalized;
            // Атакуємо
            mob.Attack(target);
        }
    }

    private bool IsTargetInRange()
    {
        return target != null &&
               Vector2.Distance(mob.Position, target.Position) <= attackRange;
    }
}
using UnityEngine;

public class MobChaseState : State
{
    private MobEntity mob;
    private CharacterEntity target;
    private float attackRange = 2f;
    private NetworkMovementComponent movement;
    private float updateTargetInterval = 0.5f;
    private float nextUpdateTime;

    public MobChaseState(MobEntity owner) : base(owner)
    {
        mob = owner;
        movement = owner.GetComponent<NetworkMovementComponent>();
    }

    public override void Enter()
    {
        target = ObjectManager.Instance.GetNearestPlayer(mob.Position);
    }

    public override void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            if (target == null)
            {
                target = ObjectManager.Instance.GetNearestPlayer(mob.Position);
                if (target == null)
                {
                    mob.stateMachine.SetState<MobIdleState>();
                    return;
                }
            }
            nextUpdateTime = Time.time + updateTargetInterval;
        }

        if (target != null)
        {
            float distance = Vector2.Distance(mob.Position, target.Position);

            if (distance <= attackRange)
            {
                mob.stateMachine.SetState<MobAttackState>();
            }
            else
            {
                // Використовуємо NetworkMovementComponent для руху
                movement.MoveToPosition(target.Position);
            }
        }
    }
}
using UnityEngine;

public class MobChaseState : State
{
    private MobEntity mob;
    private CharacterEntity target;
    private float attackRange = 2f;
    private NetworkMovementComponent movement;

    public MobChaseState(MobEntity owner) : base(owner)
    {
        mob = owner;
        movement = owner.GetComponent<NetworkMovementComponent>();
    }

    public override void Update()
    {
        if (target == null)
        {
            mob.stateMachine.SetState<MobIdleState>();
            return;
        }

        float distance = Vector2.Distance(mob.Position, target.Position);

        if (distance <= attackRange)
        {
            //mob.stateMachine.SetState<MobAttackState>();
        }
        else
        {
            // Використовуємо NetworkMovementComponent для руху
            movement.MoveToPosition(target.Position);
        }
    }
}
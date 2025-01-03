using UnityEngine;

public class MobChaseState : State
{
    private MobEntity mob;
    private CharacterEntity target;
    private float attackRange = 2f;

    public MobChaseState(MobEntity owner) : base(owner)
    {
        mob = owner;
    }

    public override void Update()
    {
        if (target == null)
        {
            mob.GetComponent<StateMachine>().SetState<MobIdleState>();
            return;
        }

        float distance = Vector2.Distance(mob.Position, target.Position);

        if (distance <= attackRange)
        {
            //mob.GetComponent<StateMachine>().SetState<MobAttackState>();
        }
        else
        {
            // Логіка переслідування цілі
        }
    }
}
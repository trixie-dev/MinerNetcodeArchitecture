using UnityEngine;

public class MobIdleState : State
{
    private MobEntity mob;
    private float searchRadius = 10f;
    private float searchInterval = 0.5f;
    private float nextSearchTime;

    public MobIdleState(MobEntity owner) : base(owner)
    {
        mob = owner;
    }

    public override void Update()
    {
        if (Time.time >= nextSearchTime)
        {
            // Пошук найближчого гравця
            if (FindNearestPlayer())
            {
                mob.GetComponent<StateMachine>().SetState<MobChaseState>();
            }
            nextSearchTime = Time.time + searchInterval;
        }
    }

    private bool FindNearestPlayer()
    {
        // Реалізація пошуку гравця
        return false;
    }
}
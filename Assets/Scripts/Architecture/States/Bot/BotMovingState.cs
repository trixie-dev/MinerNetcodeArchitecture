using UnityEngine;

public class BotMovingState : State
{
    private BotEntity bot;
    private Vector2 targetPosition;
    private float arrivalDistance = 0.1f;
    private NetworkMovementComponent movement;

    public BotMovingState(BotEntity owner) : base(owner)
    {
        bot = owner;
        movement = owner.GetComponent<NetworkMovementComponent>();
    }

    public override void Enter()
    {
        targetPosition = FindDropPoint();
    }

    public override void Update()
    {
        if (Vector2.Distance(bot.Position, targetPosition) <= arrivalDistance)
        {
            bot.stateMachine.SetState<BotIdleState>();
            return;
        }

        movement.MoveToPosition(targetPosition);
    }

    private Vector2 FindDropPoint()
    {
        return Random.insideUnitCircle * 10f;
    }
}
using UnityEngine;

public class BotIdleState : State
{
    private BotEntity bot;
    private float idleTime;
    private float maxIdleTime = 3f;

    public BotIdleState(BotEntity owner) : base(owner)
    {
        bot = owner;
    }

    public override void Enter()
    {
        idleTime = 0f;
    }

    public override void Update()
    {
        idleTime += Time.deltaTime;

        if (idleTime >= maxIdleTime)
        {
            // Перехід до збору ресурсів
            bot.GetComponent<StateMachine>().SetState<BotGatheringState>();
        }
    }
}
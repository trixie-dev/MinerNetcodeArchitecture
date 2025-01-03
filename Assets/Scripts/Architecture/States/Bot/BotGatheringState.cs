using UnityEngine;

public class BotGatheringState : State
{
    private BotEntity bot;
    private float gatheringTime;
    private float gatheringDuration = 2f;

    public BotGatheringState(BotEntity owner) : base(owner)
    {
        bot = owner;
    }

    public override void Enter()
    {
        gatheringTime = 0f;
    }

    public override void Update()
    {
        gatheringTime += Time.deltaTime;

        if (gatheringTime >= gatheringDuration)
        {
            // Спроба додати ресурс
            if (!bot.AddResource(10f))
            {
                // Якщо рюкзак повний, перейти до іншого стану
                bot.stateMachine.SetState<BotMovingState>();
            }
            gatheringTime = 0f;
        }
    }
}
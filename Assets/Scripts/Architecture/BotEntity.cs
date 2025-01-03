using UnityEngine;

public class BotEntity : CharacterEntity
{
    protected StateMachine stateMachine;

    protected override void Awake()
    {
        base.Awake();
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine();

        // Додаємо стани
        stateMachine.AddState(new BotIdleState(this));
        stateMachine.AddState(new BotGatheringState(this));
        //stateMachine.AddState(new BotMovingState(this));

        // Встановлюємо початковий стан
        stateMachine.SetState<BotIdleState>();
    }

    protected override void UpdateLogic()
    {
        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }
}
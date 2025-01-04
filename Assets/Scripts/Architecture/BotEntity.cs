using UnityEngine;

public class BotEntity : CharacterEntity
{
    private NetworkMovementComponent movement;
    private AttackComponent attackComponent;
    public StateMachine stateMachine { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<NetworkMovementComponent>();
        attackComponent = GetComponent<AttackComponent>();
        InitializeStateMachine();

        // Налаштування базових характеристик бота
        if (stats != null)
        {
            stats.ModifyBaseStat("health", 75f);
            stats.ModifyBaseStat("movespeed", 4f);
            stats.ModifyBaseStat("armor", 10f);

            stats.OnHealthChanged += HandleHealthChanged;
            stats.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnHealthChanged -= HandleHealthChanged;
            stats.OnDeath -= HandleDeath;
        }
    }

    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine();

        // Додаємо стани
        stateMachine.AddState(new BotIdleState(this));
        stateMachine.AddState(new BotGatheringState(this));
        stateMachine.AddState(new BotMovingState(this));
        stateMachine.AddState(new BotCombatState(this));

        // Встановлюємо початковий стан
        stateMachine.SetState<BotIdleState>();
    }

    public void Attack(Entity target)
    {
        // Бот може атакувати тільки мобів
        if (target is MobEntity)
        {
            attackComponent?.TryAttack(target);
        }
    }

    protected override void UpdateLogic()
    {
        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    protected override void RegisterInManager()
    {
        ObjectManager.Instance.RegisterBot(this);
    }

    protected override void UnregisterFromManager()
    {
        ObjectManager.Instance.UnregisterBot(this);
    }

    private void HandleHealthChanged(float newHealth)
    {
        // Якщо здоров'я низьке, можна перейти в стан втечі
        if (newHealth < stats.Health * 0.3f)
        {
            stateMachine.SetState<BotMovingState>();
        }
    }

    private void HandleDeath()
    {
        if (IsServer)
        {
            // Дропаємо ресурси перед знищенням
            if (backpack != null)
            {
                backpack.DropAllResources();
            }
            NetworkObject.Despawn(true);
        }
    }
}
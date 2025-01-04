using UnityEngine;

[RequireComponent(typeof(NetworkMovementComponent))]
[RequireComponent(typeof(AttackComponent))]
public class MobEntity : Entity
{
    [Header("Mob Properties")]
    [SerializeField] private float attackRange = 2f;

    private NetworkMovementComponent movement;
    private AttackComponent attackComponent;
    public StateMachine stateMachine { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<NetworkMovementComponent>();
        attackComponent = GetComponent<AttackComponent>();
        InitializeStateMachine();

        // Налаштування базових характеристик моба
        if (stats != null)
        {
            stats.ModifyBaseStat("health", 50f);
            stats.ModifyBaseStat("movespeed", 3f);
            stats.ModifyBaseStat("armor", 5f);

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
        stateMachine.AddState(new MobIdleState(this));
        stateMachine.AddState(new MobChaseState(this));
        stateMachine.AddState(new MobAttackState(this));

        // Встановлюємо початковий стан
        stateMachine.SetState<MobIdleState>();
    }

    public virtual void Attack(Entity target)
    {
        // Моб може атакувати тільки гравців і ботів
        if (target is CharacterEntity)
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
        ObjectManager.Instance.RegisterMob(this);
    }

    protected override void UnregisterFromManager()
    {
        ObjectManager.Instance.UnregisterMob(this);
    }

    private void HandleHealthChanged(float newHealth)
    {
        // Тут можна додати візуальні ефекти отримання шкоди
        // Наприклад, зміна кольору спрайту або програвання анімації
    }

    private void HandleDeath()
    {
        if (IsServer)
        {
            // Можливо, дропнути якісь ресурси перед знищенням
            NetworkObject.Despawn(true);
        }
    }

    // Публічні методи для доступу до характеристик
    public float GetHealth() => stats.Health;
    public float GetDamage() => stats.Damage;
    public float GetMoveSpeed() => stats.MoveSpeed;
}
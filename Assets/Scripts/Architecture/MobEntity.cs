using UnityEngine;

public class MobEntity : Entity
{
    [Header("Mob Properties")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;

    private float nextAttackTime;
    public StateMachine stateMachine { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine();

        // Додаємо стани
        stateMachine.AddState(new MobIdleState(this));
        stateMachine.AddState(new MobChaseState(this));
        //stateMachine.AddState(new MobAttackState(this));

        // Встановлюємо початковий стан
        stateMachine.SetState<MobIdleState>();
    }

    public virtual void Attack(CharacterEntity target)
    {
        if (!IsServer) return;

        if (Time.time >= nextAttackTime &&
            Vector2.Distance(Position, target.Position) <= attackRange)
        {
            target.TakeBackpackDamage(attackDamage);
            nextAttackTime = Time.time + attackCooldown;
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
}
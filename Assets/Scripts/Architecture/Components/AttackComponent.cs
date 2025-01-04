using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class AttackComponent : NetworkBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;

    private float nextAttackTime;
    private EntityStatsComponent stats;

    // Події
    public event UnityAction<Entity> OnAttackPerformed;
    public event UnityAction OnAttackFailed;

    private void Awake()
    {
        stats = GetComponent<EntityStatsComponent>();
    }

    public bool CanAttack()
    {
        return Time.time >= nextAttackTime;
    }

    public bool IsInRange(Vector2 targetPosition)
    {
        return Vector2.Distance(transform.position, targetPosition) <= attackRange;
    }

    public void TryAttack(Entity target)
    {
        // Якщо це серверний об'єкт, виконуємо атаку напряму
        if (IsServer)
        {
            PerformAttack(target);
            return;
        }

        // Якщо це клієнтський об'єкт, відправляємо ServerRpc
        if (IsOwner)
        {
            RequestAttackServerRpc(target.NetworkObjectId);
        }
    }

    private void PerformAttack(Entity target)
    {
        if (!CanAttack() || target == null)
        {
            OnAttackFailed?.Invoke();
            return;
        }

        if (!IsInRange(target.Position))
        {
            OnAttackFailed?.Invoke();
            return;
        }

        // Перевіряємо тип атакуючого і цілі
        var attacker = GetComponent<Entity>();
        bool canAttack = false;

        if (attacker is Player)
        {
            // Гравець може атакувати тільки мобів
            canAttack = target is MobEntity;
        }
        else if (attacker is MobEntity)
        {
            // Моб може атакувати тільки гравців і ботів
            canAttack = target is CharacterEntity;
        }
        else if (attacker is BotEntity)
        {
            // Бот може атакувати тільки мобів
            canAttack = target is MobEntity;
        }

        if (!canAttack)
        {
            OnAttackFailed?.Invoke();
            return;
        }

        // Використовуємо характеристики з EntityStatsComponent
        float damage = stats != null ? stats.Damage : baseDamage;

        // Наносимо шкоду через EntityStats
        if (target.TryGetComponent<EntityStatsComponent>(out var targetStats))
        {
            targetStats.ApplyDamage(damage);
        }

        // Оновлюємо час наступної атаки
        float actualCooldown = stats != null ? 1f / stats.AttackSpeed : attackCooldown;
        nextAttackTime = Time.time + actualCooldown;

        OnAttackPerformed?.Invoke(target);
        NotifyAttackClientRpc(target.NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)] // Дозволяємо виклик без власності
    private void RequestAttackServerRpc(ulong targetNetId, ServerRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetId, out NetworkObject targetObject))
        {
            if (targetObject.TryGetComponent<Entity>(out Entity target))
            {
                PerformAttack(target);
            }
        }
    }

    [ClientRpc]
    private void NotifyAttackClientRpc(ulong targetNetId)
    {
        // Клієнтська візуалізація атаки
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetId, out NetworkObject targetObject))
        {
            if (targetObject.TryGetComponent<Entity>(out Entity target))
            {
                OnAttackPerformed?.Invoke(target);
            }
        }
    }

    // Публічні властивості
    public float AttackRange => attackRange;
    public float BaseDamage => baseDamage;
    public float Cooldown => attackCooldown;
}
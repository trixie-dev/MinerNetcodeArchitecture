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
        nextAttackTime = 0f; // Можемо атакувати відразу після спавну
    }

    public bool CanAttack()
    {
        return Time.time >= nextAttackTime;
    }

    public bool IsInRange(Vector2 targetPosition)
    {
        return Vector2.Distance(transform.position, targetPosition) <= attackRange;
    }

    private void LogAttack(Entity target, float damage, bool success, string reason = "")
    {
        // Логуємо і на клієнті, і на сервері
        string side = IsServer ? "Server" : "Client";
        string timeStamp = TimeFormatter.FormatTime(Time.time);
        string attackerName = GetComponent<Entity>()?.GetType().Name ?? "Unknown";
        string targetName = target?.GetType().Name ?? "Unknown";
        string attackerId = NetworkObject.OwnerClientId.ToString();
        string targetId = target?.GetComponent<NetworkObject>()?.OwnerClientId.ToString() ?? "None";

        if (success)
        {
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null,
                "[{0}][{1}] Attack: {2}({3}) -> {4}({5}), Damage: {6:F1}, NextAttack: {7}",
                side, timeStamp, attackerName, attackerId, targetName, targetId, damage,
                TimeFormatter.FormatTime(nextAttackTime));
        }
        else
        {
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null,
                "[{0}][{1}] Attack Failed: {2}({3}) -> {4}({5}), Reason: {6}",
                side, timeStamp, attackerName, attackerId, targetName, targetId, reason);
        }
    }

    public void TryAttack(Entity target)
    {
        Debug.Log($"TryAttack called on {(IsServer ? "Server" : "Client")}"); // DEBUG

        if (!CanAttack())
        {
            Debug.Log($"Attack blocked by cooldown. Time: {Time.time}, NextAttack: {nextAttackTime}"); // DEBUG
            LogAttack(target, 0, false, "Cooldown");
            OnAttackFailed?.Invoke();
            return;
        }

        // Якщо це серверний об'єкт, виконуємо атаку напряму
        if (IsServer)
        {
            Debug.Log("Executing attack on server directly"); // DEBUG
            PerformAttack(target);
            return;
        }

        // Якщо це клієнтський об'єкт, відправляємо ServerRpc
        if (IsOwner)
        {
            Debug.Log("Sending attack request to server"); // DEBUG
            RequestAttackServerRpc(target.NetworkObjectId);
            UpdateNextAttackTime();
        }
    }

    private void PerformAttack(Entity target)
    {
        Debug.Log($"PerformAttack called on {(IsServer ? "Server" : "Client")}"); // DEBUG

        if (!CanAttack() || target == null)
        {
            string reason = target == null ? "No Target" : "Cooldown";
            Debug.Log($"Attack failed: {reason}"); // DEBUG
            LogAttack(target, 0, false, reason);
            OnAttackFailed?.Invoke();
            return;
        }

        if (!IsInRange(target.Position))
        {
            LogAttack(target, 0, false, "Out of Range");
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
            LogAttack(target, 0, false, "Invalid Target Type");
            OnAttackFailed?.Invoke();
            return;
        }

        // Використовуємо характеристики з EntityStatsComponent
        float damage = stats != null ? stats.Damage : baseDamage;

        // Наносимо шкоду через EntityStats
        if (target.TryGetComponent<EntityStatsComponent>(out var targetStats))
        {
            targetStats.ApplyDamage(damage);
            LogAttack(target, damage, true);
        }
        else
        {
            LogAttack(target, 0, false, "No EntityStats Component");
        }

        // Оновлюємо час наступної атаки
        UpdateNextAttackTime();

        OnAttackPerformed?.Invoke(target);
        NotifyAttackClientRpc(target.NetworkObjectId);
    }

    public void UpdateNextAttackTime()
    {
        float actualCooldown = stats != null ? 1f / stats.AttackSpeed : attackCooldown;
        nextAttackTime = Time.time + actualCooldown;
        Debug.Log($"Next attack time updated to: {TimeFormatter.FormatTime(nextAttackTime)}"); // DEBUG
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestAttackServerRpc(ulong targetNetId, ServerRpcParams rpcParams = default)
    {
        Debug.Log("ServerRpc received"); // DEBUG

        if (!CanAttack())
        {
            Debug.Log($"ServerRpc rejected: cooldown not ready. Time: {Time.time}, NextAttack: {nextAttackTime}"); // DEBUG
            return;
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetId, out NetworkObject targetObject))
        {
            if (targetObject.TryGetComponent<Entity>(out Entity target))
            {
                PerformAttack(target);
            }
            else
            {
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null,
                    "[Server] Attack request rejected: Target {0} has no Entity component",
                    targetNetId);
            }
        }
        else
        {
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null,
                "[Server] Attack request rejected: Target {0} not found",
                targetNetId);
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
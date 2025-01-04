using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(EntityStatsComponent))]
public abstract class Entity : NetworkBehaviour
{
    [Header("Base Entity Properties")]
    [SerializeField] protected float moveSpeed = 5f;

    // Базові властивості для всіх сутностей
    public Vector2 Position => transform.position;

    protected EntityStatsComponent stats;

    // Додаємо віртуальний Awake
    protected virtual void Awake()
    {
        stats = GetComponent<EntityStatsComponent>();
        // Базова ініціалізація
    }

    // Абстрактний метод для оновлення логіки
    protected abstract void UpdateLogic();

    protected virtual void Update()
    {
        if (!IsSpawned) return;
        UpdateLogic();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        RegisterInManager();
    }

    public override void OnNetworkDespawn()
    {
        UnregisterFromManager();
        base.OnNetworkDespawn();
    }

    protected virtual void RegisterInManager() { }
    protected virtual void UnregisterFromManager() { }
}

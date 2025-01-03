using Unity.Netcode;
using UnityEngine;

public abstract class CharacterEntity : Entity
{
    [Header("Backpack Settings")]
    [SerializeField] protected float maxBackpackCapacity = 100f;

    // Мережева змінна для синхронізації місткості рюкзака
    private NetworkVariable<float> netCurrentCapacity = new NetworkVariable<float>();

    // Властивості рюкзака
    public float MaxBackpackCapacity => maxBackpackCapacity;
    public float CurrentCapacity => IsServer ? currentCapacity : netCurrentCapacity.Value;

    protected float currentCapacity;

    
    protected override void Awake()
    {
        currentCapacity = maxBackpackCapacity;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            netCurrentCapacity.Value = currentCapacity;
        }

        netCurrentCapacity.OnValueChanged += OnCapacityChanged;
    }

    public override void OnNetworkDespawn()
    {
        netCurrentCapacity.OnValueChanged -= OnCapacityChanged;
        base.OnNetworkDespawn();
    }

    // Метод для отримання шкоди рюкзаку
    public virtual void TakeBackpackDamage(float damage)
    {
        if (!IsServer) return;

        currentCapacity = Mathf.Max(0, currentCapacity - damage);
        netCurrentCapacity.Value = currentCapacity;

        if (currentCapacity <= 0)
        {
            DropAllResources();
        }
    }

    // Метод для додавання ресурсів у рюкзак
    public virtual bool AddResource(float amount)
    {
        if (!IsServer) return false;

        if (currentCapacity + amount <= maxBackpackCapacity)
        {
            currentCapacity += amount;
            netCurrentCapacity.Value = currentCapacity;
            return true;
        }
        return false;
    }

    protected virtual void OnCapacityChanged(float previousValue, float newValue)
    {
        // Можна перевизначити для додаткової логіки
    }

    protected virtual void DropAllResources()
    {
        // Реалізація викидання всіх ресурсів
    }
}
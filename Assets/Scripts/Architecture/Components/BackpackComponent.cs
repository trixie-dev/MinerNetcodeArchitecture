using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class BackpackComponent : NetworkBehaviour
{
    [Header("Backpack Settings")]
    [SerializeField] private float maxCapacity = 100f;
    [SerializeField] private float repairRate = 5f; // Швидкість відновлення місткості

    // Мережеві змінні
    private NetworkVariable<float> netCurrentCapacity = new NetworkVariable<float>();
    private NetworkVariable<float> netMaxCapacity = new NetworkVariable<float>();
    private NetworkVariable<float> netResourceAmount = new NetworkVariable<float>();

    // Події
    public event UnityAction<float> OnCapacityChanged;
    public event UnityAction<float> OnResourceAmountChanged;
    public event UnityAction<float> OnDamageReceived;
    public event UnityAction OnBackpackFull;
    public event UnityAction OnBackpackBroke;

    // Властивості
    public float MaxCapacity => IsServer ? maxCapacity : netMaxCapacity.Value;
    public float CurrentCapacity => IsServer ? currentCapacity : netCurrentCapacity.Value;
    public float ResourceAmount => IsServer ? resourceAmount : netResourceAmount.Value;
    public bool IsFull => ResourceAmount >= CurrentCapacity;
    public bool IsBroken => CurrentCapacity <= 0;

    private float currentCapacity;
    private float resourceAmount;
    private bool isRepairing;

    private void Awake()
    {
        currentCapacity = maxCapacity;
        resourceAmount = 0f;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            netCurrentCapacity.Value = currentCapacity;
            netMaxCapacity.Value = maxCapacity;
            netResourceAmount.Value = resourceAmount;
        }

        // Підписка на зміни мережевих змінних
        netCurrentCapacity.OnValueChanged += OnNetCapacityChanged;
        netResourceAmount.OnValueChanged += OnNetResourceAmountChanged;
    }

    public override void OnNetworkDespawn()
    {
        netCurrentCapacity.OnValueChanged -= OnNetCapacityChanged;
        netResourceAmount.OnValueChanged -= OnNetResourceAmountChanged;
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (IsServer && isRepairing)
        {
            RepairCapacity();
        }
    }

    #region Server Methods
    public bool AddResource(float amount)
    {
        if (!IsServer) return false;
        if (IsBroken || amount <= 0) return false;

        float availableSpace = currentCapacity - resourceAmount;
        if (availableSpace < amount) return false;

        resourceAmount += amount;
        netResourceAmount.Value = resourceAmount;

        if (IsFull)
        {
            OnBackpackFull?.Invoke();
        }

        return true;
    }

    public bool RemoveResource(float amount)
    {
        if (!IsServer) return false;
        if (amount <= 0 || resourceAmount < amount) return false;

        resourceAmount -= amount;
        netResourceAmount.Value = resourceAmount;
        return true;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;
        if (damage <= 0) return;

        float previousCapacity = currentCapacity;
        currentCapacity = Mathf.Max(0, currentCapacity - damage);
        netCurrentCapacity.Value = currentCapacity;

        OnDamageReceived?.Invoke(damage);

        // Якщо рюкзак зламався, викидаємо всі ресурси
        if (!IsBroken && currentCapacity <= 0)
        {
            DropAllResources();
            OnBackpackBroke?.Invoke();
        }
        // Якщо місткість стала менше ніж ресурсів - викидаємо надлишок
        else if (resourceAmount > currentCapacity)
        {
            float excessAmount = resourceAmount - currentCapacity;
            resourceAmount = currentCapacity;
            netResourceAmount.Value = resourceAmount;
            DropResources(excessAmount);
        }
    }

    public void StartRepairing()
    {
        if (!IsServer) return;
        isRepairing = true;
    }

    public void StopRepairing()
    {
        if (!IsServer) return;
        isRepairing = false;
    }

    private void RepairCapacity()
    {
        if (currentCapacity >= maxCapacity) return;

        currentCapacity = Mathf.Min(maxCapacity, currentCapacity + repairRate * Time.deltaTime);
        netCurrentCapacity.Value = currentCapacity;
    }

    public void DropAllResources()
    {
        if (resourceAmount > 0)
        {
            DropResources(resourceAmount);
            resourceAmount = 0;
            netResourceAmount.Value = 0;
        }
    }

    private void DropResources(float amount)
    {
        // Тут можна створити об'єкт ресурсів на землі
        // Наприклад, спавн префабу з ресурсами
        Vector2 dropPosition = (Vector2)transform.position + Random.insideUnitCircle;
        // GameManager.Instance.SpawnDroppedResource(dropPosition, amount);
    }
    #endregion

    #region Network Callbacks
    private void OnNetCapacityChanged(float previousValue, float newValue)
    {
        OnCapacityChanged?.Invoke(newValue);
    }

    private void OnNetResourceAmountChanged(float previousValue, float newValue)
    {
        OnResourceAmountChanged?.Invoke(newValue);
    }
    #endregion
}
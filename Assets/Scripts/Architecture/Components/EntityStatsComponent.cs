using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class EntityStatsComponent : NetworkBehaviour
{
    [System.Serializable]
    public class StatValue
    {
        public float baseValue;
        public float minValue;
        public float maxValue;
        private float currentValue;

        public float CurrentValue
        {
            get => currentValue;
            set => currentValue = Mathf.Clamp(value, minValue, maxValue);
        }

        public StatValue(float baseValue, float minValue, float maxValue)
        {
            this.baseValue = baseValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.currentValue = baseValue;
        }
    }

    [Header("Base Stats")]
    [SerializeField] private float baseHealth = 100f;
    [SerializeField] private float baseStamina = 100f;
    [SerializeField] private float baseArmor = 0f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseMoveSpeed = 5f;

    // Мережеві змінні для синхронізації
    private NetworkVariable<float> netHealth = new NetworkVariable<float>();
    private NetworkVariable<float> netStamina = new NetworkVariable<float>();

    // Події
    public event UnityAction<float> OnHealthChanged;
    public event UnityAction<float> OnStaminaChanged;
    public event UnityAction OnDeath;

    // Характеристики
    private StatValue health;
    private StatValue stamina;
    private StatValue armor;
    private StatValue damage;
    private StatValue attackSpeed;
    private StatValue moveSpeed;

    // Властивості для доступу до характеристик
    public float Health => IsServer ? health.CurrentValue : netHealth.Value;
    public float Stamina => IsServer ? stamina.CurrentValue : netStamina.Value;
    public float Armor => armor.CurrentValue;
    public float Damage => damage.CurrentValue;
    public float AttackSpeed => attackSpeed.CurrentValue;
    public float MoveSpeed => moveSpeed.CurrentValue;

    private void Awake()
    {
        InitializeStats();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            netHealth.Value = health.CurrentValue;
            netStamina.Value = stamina.CurrentValue;
        }

        // Підписка на зміни мережевих змінних
        netHealth.OnValueChanged += OnNetHealthChanged;
        netStamina.OnValueChanged += OnNetStaminaChanged;
    }

    public override void OnNetworkDespawn()
    {
        netHealth.OnValueChanged -= OnNetHealthChanged;
        netStamina.OnValueChanged -= OnNetStaminaChanged;
        base.OnNetworkDespawn();
    }

    private void InitializeStats()
    {
        health = new StatValue(baseHealth, 0, baseHealth);
        stamina = new StatValue(baseStamina, 0, baseStamina);
        armor = new StatValue(baseArmor, 0, 100);
        damage = new StatValue(baseDamage, 0, 1000);
        attackSpeed = new StatValue(baseAttackSpeed, 0.1f, 10);
        moveSpeed = new StatValue(baseMoveSpeed, 0, 20);
    }

    #region Server Methods
    public void ModifyHealth(float amount)
    {
        if (!IsServer) return;

        float previousHealth = health.CurrentValue;
        health.CurrentValue += amount;
        netHealth.Value = health.CurrentValue;

        if (health.CurrentValue <= 0 && previousHealth > 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void ModifyStamina(float amount)
    {
        if (!IsServer) return;

        stamina.CurrentValue += amount;
        netStamina.Value = stamina.CurrentValue;
    }

    public void ApplyDamage(float damage, float armorPenetration = 0)
    {
        if (!IsServer) return;

        float effectiveArmor = Mathf.Max(0, armor.CurrentValue - armorPenetration);
        float damageReduction = effectiveArmor / (effectiveArmor + 100); // Формула зменшення шкоди
        float finalDamage = damage * (1 - damageReduction);

        ModifyHealth(-finalDamage);
    }

    public bool UseStamina(float amount)
    {
        if (!IsServer) return false;
        if (stamina.CurrentValue < amount) return false;

        ModifyStamina(-amount);
        return true;
    }
    #endregion

    #region Stat Modifiers
    public void ModifyBaseStat(string statName, float modifier)
    {
        if (!IsServer) return;

        StatValue stat = GetStatByName(statName);
        if (stat != null)
        {
            stat.baseValue += modifier;
            stat.CurrentValue = stat.baseValue;
        }
    }

    private StatValue GetStatByName(string statName)
    {
        return statName.ToLower() switch
        {
            "health" => health,
            "stamina" => stamina,
            "armor" => armor,
            "damage" => damage,
            "attackspeed" => attackSpeed,
            "movespeed" => moveSpeed,
            _ => null
        };
    }
    #endregion

    #region Network Callbacks
    private void OnNetHealthChanged(float previousValue, float newValue)
    {
        OnHealthChanged?.Invoke(newValue);
    }

    private void OnNetStaminaChanged(float previousValue, float newValue)
    {
        OnStaminaChanged?.Invoke(newValue);
    }
    #endregion
}
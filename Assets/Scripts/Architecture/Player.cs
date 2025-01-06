using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkMovementComponent))]
[RequireComponent(typeof(InputComponent))]
[RequireComponent(typeof(AttackComponent))]
[RequireComponent(typeof(EffectsComponent))]
public class Player : PlayerEntity
{
    private AttackComponent attackComponent;
    private SpeedBooster speedBooster;
    private AoEStunner aoeStunner;

    protected override void Awake()
    {
        base.Awake();
        attackComponent = GetComponent<AttackComponent>();

        // Ініціалізуємо предмети
        speedBooster = new SpeedBooster();
        aoeStunner = new AoEStunner();

        // Підписуємося на події вводу
        if (input != null)
        {
            input.OnMoveInput += HandleMoveInput;
            input.OnInteractInput += HandleInteractInput;
            input.OnAttackInput += HandleAttackInput;
            input.OnInventoryInput += HandleInventoryInput;
            input.OnUseSpeedBooster += HandleSpeedBooster;
            input.OnUseStunner += HandleStunner;
        }

        if (stats != null)
        {
            stats.OnHealthChanged += HandleHealthChanged;
            stats.OnStaminaChanged += HandleStaminaChanged;
            stats.OnDeath += HandleDeath;
        }

        if (backpack != null)
        {
            backpack.OnCapacityChanged += HandleCapacityChanged;
            backpack.OnResourceAmountChanged += HandleResourceAmountChanged;
            backpack.OnDamageReceived += HandleBackpackDamage;
            backpack.OnBackpackBroke += HandleBackpackBroke;
        }
    }

    private void OnDestroy()
    {
        // Відписуємося від подій
        if (input != null)
        {
            input.OnMoveInput -= HandleMoveInput;
            input.OnInteractInput -= HandleInteractInput;
            input.OnAttackInput -= HandleAttackInput;
            input.OnInventoryInput -= HandleInventoryInput;
            input.OnUseSpeedBooster -= HandleSpeedBooster;
            input.OnUseStunner -= HandleStunner;
        }

        if (stats != null)
        {
            stats.OnHealthChanged -= HandleHealthChanged;
            stats.OnStaminaChanged -= HandleStaminaChanged;
            stats.OnDeath -= HandleDeath;
        }

        if (backpack != null)
        {
            backpack.OnCapacityChanged -= HandleCapacityChanged;
            backpack.OnResourceAmountChanged -= HandleResourceAmountChanged;
            backpack.OnDamageReceived -= HandleBackpackDamage;
            backpack.OnBackpackBroke -= HandleBackpackBroke;
        }
    }

    #region Input Handlers
    private void HandleMoveInput(Vector2 moveDirection)
    {
        // Передаємо ввід до NetworkMovementComponent
        movement?.HandlePlayerInput(moveDirection);
    }

    private void HandleInteractInput()
    {
        if (!IsOwner) return;

        // Пошук найближчого ресурсу для взаємодії
        var nearestResource = ObjectManager.Instance.GetNearestResource(transform.position);
        if (nearestResource != null)
        {
            float distance = Vector2.Distance(transform.position, nearestResource.Position);
            if (distance <= 2f) // Радіус взаємодії
            {
                // Тут буде логіка взаємодії з ресурсом
                RequestInteractServerRpc();
            }
        }
    }

    private void HandleAttackInput()
    {
        if (!IsOwner) return;

        Debug.Log("HandleAttackInput called"); // DEBUG

        var equipment = GetComponent<EquipmentComponent>();
        if (equipment == null)
        {
            Debug.LogError("No EquipmentComponent found"); // DEBUG
            return;
        }

        // Спочатку перевіряємо, чи дивимось на ресурс
        var nearestResource = ObjectManager.Instance.GetNearestResource(transform.position);
        if (nearestResource != null && Vector2.Distance(transform.position, nearestResource.Position) <= attackComponent.AttackRange)
        {
            Debug.Log($"Attempting to mine resource at distance {Vector2.Distance(transform.position, nearestResource.Position)}"); // DEBUG

            // Перевіряємо чи можемо атакувати
            if (!attackComponent.CanAttack())
            {
                Debug.Log("Mining blocked by cooldown"); // DEBUG
                return;
            }

            float miningDamage = equipment.CurrentPickaxe?.Damage ?? 1f;
            Debug.Log($"Mining damage: {miningDamage} (Pickaxe: {equipment.CurrentPickaxe?.Name ?? "None"})"); // DEBUG

            if (nearestResource.TryHarvest(miningDamage, HandleResourceHarvested))
            {
                attackComponent.UpdateNextAttackTime();
            }
            return;
        }

        // Якщо не ресурс, шукаємо моба
        var nearestMob = ObjectManager.Instance.GetNearestMob(transform.position);
        if (nearestMob != null)
        {
            Debug.Log($"Found nearest mob at distance {Vector2.Distance(transform.position, nearestMob.Position)}"); // DEBUG
            if (attackComponent.IsInRange(nearestMob.Position))
            {
                Debug.Log("Attempting to attack mob"); // DEBUG
                if (stats.UseStamina(10f))
                {
                    attackComponent.TryAttack(nearestMob);
                }
                else
                {
                    Debug.Log("Not enough stamina to attack"); // DEBUG
                }
            }
            else
            {
                Debug.Log("Mob is out of range"); // DEBUG
            }
        }
        else
        {
            Debug.Log("No targets found"); // DEBUG
        }
    }

    private void HandleResourceHarvested(float amount)
    {
        if (AddResource(amount))
        {
            DebugUtil.LogGameplay($"Added {amount} resources to backpack");
        }
        else
        {
            DebugUtil.LogGameplay("Failed to add resources to backpack", LogType.Warning);
        }
    }

    private void HandleInventoryInput()
    {
        if (!IsOwner) return;
        // Тут буде логіка відкриття/закриття інвентаря
    }
    #endregion

    #region Network RPCs
    [ServerRpc]
    private void RequestInteractServerRpc(ServerRpcParams rpcParams = default)
    {
        // Серверна валідація взаємодії
        var nearestResource = ObjectManager.Instance.GetNearestResource(transform.position);
        if (nearestResource != null && Vector2.Distance(transform.position, nearestResource.Position) <= 2f)
        {
            if (nearestResource.TryHarvest(10f))
            {
                AddResource(10f);
            }
        }
    }

    [ServerRpc]
    private void RequestAttackServerRpc()
    {
        // Наносимо шкоду цілі
        //var target = GetTarget();
        //if (target != null)
        //{
        //    target.GetComponent<EntityStatsComponent>()?.ApplyDamage(stats.Damage);
        //}
    }
    #endregion

    private void HandleHealthChanged(float newHealth)
    {
        // Оновлення UI здоров'я
    }

    private void HandleStaminaChanged(float newStamina)
    {
        // Оновлення UI витривалості
    }

    private void HandleDeath()
    {
        // Логіка смерті гравця
    }

    private void HandleCapacityChanged(float newCapacity)
    {
        // Оновлення UI місткості рюкзака
    }

    private void HandleResourceAmountChanged(float newAmount)
    {
        // Оновлення UI кількості ресурсів
    }

    private void HandleBackpackDamage(float damage)
    {
        // Візуальний ефект пошкодження рюкзака
    }

    private void HandleBackpackBroke()
    {
        // Логіка поломки рюкзака
    }

    private void HandleSpeedBooster()
    {
        if (!IsOwner) return;

        if (speedBooster.CanUse(Time.time))
        {
            RequestUseSpeedBoosterServerRpc();
        }
    }

    private void HandleStunner()
    {
        if (!IsOwner) return;

        if (aoeStunner.CanUse(Time.time))
        {
            RequestUseStunnerServerRpc();
        }
    }

    [ServerRpc]
    private void RequestUseSpeedBoosterServerRpc()
    {
        if (speedBooster.CanUse(Time.time))
        {
            speedBooster.Use(this);
            speedBooster.StartCooldown(Time.time);
        }
    }

    [ServerRpc]
    private void RequestUseStunnerServerRpc()
    {
        if (aoeStunner.CanUse(Time.time))
        {
            aoeStunner.Use(this);
            aoeStunner.StartCooldown(Time.time);
        }
    }
}
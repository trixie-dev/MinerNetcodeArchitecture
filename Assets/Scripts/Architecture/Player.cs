using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkMovementComponent))]
[RequireComponent(typeof(InputComponent))]
public class Player : PlayerEntity
{
    protected override void Awake()
    {
        base.Awake();

        // Підписуємося на події вводу
        if (input != null)
        {
            input.OnMoveInput += HandleMoveInput;
            input.OnInteractInput += HandleInteractInput;
            input.OnAttackInput += HandleAttackInput;
            input.OnInventoryInput += HandleInventoryInput;
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

        // Перевірка витривалості перед атакою
        if (stats.UseStamina(10f))
        {
            // Виконуємо атаку
            RequestAttackServerRpc();
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
}
using Unity.Netcode;
using UnityEngine;

public abstract class PlayerEntity : CharacterEntity
{
    protected NetworkMovementComponent movement;
    protected InputComponent input;

    protected override void Awake()
    {
        base.Awake();

        // Отримуємо необхідні компоненти
        movement = GetComponent<NetworkMovementComponent>();
        input = GetComponent<InputComponent>();

        if (movement == null || input == null)
        {
            Debug.LogError($"Missing required components on {gameObject.name}");
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Вмикаємо/вимикаємо компонент вводу залежно від того, чи є ми власником
        if (input != null)
        {
            input.enabled = IsOwner;
        }
    }

    protected override void RegisterInManager()
    {
        ObjectManager.Instance.RegisterPlayer(this);
    }

    protected override void UnregisterFromManager()
    {
        ObjectManager.Instance.UnregisterPlayer(this);
    }

    protected override void UpdateLogic()
    {
        if (IsOwner)
        {
            // Базова логіка оновлення для власника
            HandleInput();
        }
    }

    protected virtual void HandleInput()
    {
        // Перевизначається в нащадках для специфічної обробки вводу
    }
}
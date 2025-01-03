using Unity.Netcode;
using UnityEngine;

public abstract class Entity : NetworkBehaviour
{
    [Header("Base Entity Properties")]
    [SerializeField] protected float moveSpeed = 5f;

    // Базові властивості для всіх сутностей
    public Vector2 Position => transform.position;

    // Додаємо віртуальний Awake
    protected virtual void Awake()
    {
        // Базова ініціалізація
    }

    // Абстрактний метод для оновлення логіки
    protected abstract void UpdateLogic();

    protected virtual void Update()
    {
        if (!IsSpawned) return;
        UpdateLogic();
    }
}

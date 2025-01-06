using UnityEngine;
using UnityEngine.Events;

public class InputComponent : MonoBehaviour
{
    // Події для різних типів вводу
    public event UnityAction<Vector2> OnMoveInput;
    public event UnityAction OnInteractInput;
    public event UnityAction OnAttackInput;
    public event UnityAction OnInventoryInput;
    public event UnityAction OnUseSpeedBooster;
    public event UnityAction OnUseStunner;

    // Поточний стан вводу
    public Vector2 MoveInput { get; private set; }
    public bool IsInteracting { get; private set; }
    public bool IsAttacking { get; private set; }

    private void Update()
    {
        // Обробка руху
        Vector2 moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // Завжди відправляємо поточний стан руху
        OnMoveInput?.Invoke(moveInput);
        MoveInput = moveInput;

        // Обробка взаємодії (наприклад, клавіша E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            IsInteracting = true;
            OnInteractInput?.Invoke();
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            IsInteracting = false;
        }

        // Обробка атаки (ліва кнопка миші)
        if (Input.GetMouseButtonDown(0))
        {
            IsAttacking = true;
            OnAttackInput?.Invoke();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            IsAttacking = false;
        }

        // Відкриття інвентаря (клавіша I або Tab)
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
        {
            OnInventoryInput?.Invoke();
        }

        // Використання предметів
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnUseSpeedBooster?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            OnUseStunner?.Invoke();
        }
    }
}
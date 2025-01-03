using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
public class NetworkMovementComponent : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    private NetworkTransform networkTransform;

    // Властивості для AI
    public bool IsMoving { get; private set; }
    public Vector2 CurrentMoveDirection { get; private set; }

    private void Awake()
    {
        networkTransform = GetComponent<NetworkTransform>();
        // Дозволяємо клієнту-власнику керувати об'єктом
        networkTransform.InLocalSpace = false;
        networkTransform.Interpolate = true;
        networkTransform.SyncPositionX = networkTransform.SyncPositionY = true;
        networkTransform.SyncRotAngleZ = true;
    }

    // Метод для руху гравця (керованого вводом)
    public void HandlePlayerInput(Vector2 input)
    {
        // Дозволяємо рух як власнику, так і серверу
        if (!IsOwner && !IsServer) return;

        if (input != Vector2.zero)
        {
            Move(input);
            Rotate(input);
        }
    }

    // Метод для руху AI (для ботів і мобів)
    public void MoveToPosition(Vector2 targetPos)
    {
        if (!IsServer) return;

        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        if (Vector2.Distance(transform.position, targetPos) > 0.1f)
        {
            Move(direction);
            Rotate(direction);
            IsMoving = true;
        }
        else
        {
            IsMoving = false;
        }
    }

    private void Move(Vector2 direction)
    {
        CurrentMoveDirection = direction;
        Vector2 movement = direction * moveSpeed * Time.deltaTime;
        transform.position += (Vector3)movement;
    }

    private void Rotate(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float rotation = Mathf.MoveTowardsAngle(
            transform.eulerAngles.z,
            targetAngle,
            rotationSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }
}
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkMovementComponent : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float interpolationFactor = 15f;

    // Мережеві змінні для синхронізації
    private NetworkVariable<Vector2> netPosition = new NetworkVariable<Vector2>();
    private NetworkVariable<float> netRotation = new NetworkVariable<float>();

    // Локальні змінні для інтерполяції
    private Vector2 velocity;
    private Vector2 targetPosition;
    private float targetRotation;

    // Додаємо властивості для AI
    public bool IsMoving { get; private set; }
    public Vector2 CurrentMoveDirection { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            netPosition.Value = transform.position;
            netRotation.Value = transform.eulerAngles.z;
        }

        targetPosition = transform.position;
        targetRotation = transform.eulerAngles.z;
    }

    private void Update()
    {
        if (!IsOwner && !IsServer)
        {
            // Інтерполяція для не-власників
            InterpolateMovement();
        }
    }

    #region Movement Methods
    // Метод для руху гравця (керованого вводом)
    public void HandlePlayerInput(Vector2 input)
    {
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

        if (IsServer)
        {
            netPosition.Value = transform.position;
        }
        else if (IsOwner)
        {
            RequestMoveServerRpc(transform.position, transform.eulerAngles.z);
        }
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

        if (IsServer)
        {
            netRotation.Value = rotation;
        }
    }

    private void InterpolateMovement()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            netPosition.Value,
            Time.deltaTime * interpolationFactor
        );

        float currentRotation = transform.eulerAngles.z;
        float newRotation = Mathf.LerpAngle(
            currentRotation,
            netRotation.Value,
            Time.deltaTime * interpolationFactor
        );
        transform.rotation = Quaternion.Euler(0, 0, newRotation);
    }
    #endregion

    #region Network RPCs
    [ServerRpc]
    private void RequestMoveServerRpc(Vector2 newPosition, float newRotation, ServerRpcParams rpcParams = default)
    {
        // Валідація руху на сервері
        Vector2 currentPos = transform.position;
        float distance = Vector2.Distance(currentPos, newPosition);
        float maxDistance = moveSpeed * Time.deltaTime * 1.5f;

        if (distance <= maxDistance)
        {
            netPosition.Value = newPosition;
            netRotation.Value = newRotation;
            transform.position = newPosition;
            transform.rotation = Quaternion.Euler(0, 0, newRotation);
        }
        else
        {
            SyncPositionClientRpc(netPosition.Value, netRotation.Value);
        }
    }

    [ClientRpc]
    private void SyncPositionClientRpc(Vector2 position, float rotation)
    {
        if (!IsOwner) return;
        transform.position = position;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }
    #endregion
}
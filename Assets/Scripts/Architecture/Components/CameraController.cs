using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private bool boundCamera = true;
    [SerializeField] private Vector2 mapBounds = new Vector2(50f, 50f);

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float zoomSmoothTime = 0.2f;

    private Player targetPlayer;
    private Transform targetTransform;
    private Vector3 targetPosition;
    private bool followingPosition;
    private Vector3 velocity = Vector3.zero;
    private float currentZoom;
    private float targetZoom;
    private float zoomVelocity;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        currentZoom = targetZoom = mainCamera.orthographicSize;
    }

    private void Start()
    {
        // Підписуємося на події спавну гравців
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Якщо це наш клієнт, шукаємо локального гравця
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            FindLocalPlayer();
        }
    }

    private void FindLocalPlayer()
    {
        var players = FindObjectsOfType<Player>();
        foreach (var player in players)
        {
            if (player.IsOwner)
            {
                targetPlayer = player;
                break;
            }
        }
    }

    private void LateUpdate()
    {
        UpdateZoom();
        UpdatePosition();
    }

    private void UpdateZoom()
    {
        // Плавно змінюємо зум
        currentZoom = Mathf.SmoothDamp(
            currentZoom,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime
        );
        mainCamera.orthographicSize = currentZoom;
    }

    private void UpdatePosition()
    {
        Vector3 targetPos;

        if (followingPosition)
        {
            targetPos = targetPosition;
        }
        else if (targetTransform != null)
        {
            targetPos = targetTransform.position;
        }
        else if (targetPlayer != null)
        {
            targetPos = targetPlayer.transform.position;
        }
        else
        {
            FindLocalPlayer();
            return;
        }

        // Додаємо offset
        targetPos += offset;

        // Плавно переміщуємо камеру
        Vector3 smoothPosition = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            smoothTime
        );

        // Обмежуємо позицію камери межами карти
        if (boundCamera)
        {
            float height = mainCamera.orthographicSize;
            float width = height * mainCamera.aspect;

            smoothPosition.x = Mathf.Clamp(
                smoothPosition.x,
                -mapBounds.x + width,
                mapBounds.x - width
            );

            smoothPosition.y = Mathf.Clamp(
                smoothPosition.y,
                -mapBounds.y + height,
                mapBounds.y - height
            );
        }

        transform.position = smoothPosition;
    }

    #region Zoom Methods
    public void ZoomIn(float amount)
    {
        SetZoom(targetZoom - amount);
    }

    public void ZoomOut(float amount)
    {
        SetZoom(targetZoom + amount);
    }

    public void SetZoom(float zoom)
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public void SetZoomImmediate(float zoom)
    {
        targetZoom = currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        mainCamera.orthographicSize = currentZoom;
    }

    public float GetCurrentZoom()
    {
        return currentZoom;
    }
    #endregion

    #region Target Methods
    // Слідкувати за гравцем
    public void FollowPlayer(Player player)
    {
        targetPlayer = player;
        targetTransform = null;
        followingPosition = false;
    }

    // Слідкувати за трансформом
    public void FollowTransform(Transform transform)
    {
        targetTransform = transform;
        targetPlayer = null;
        followingPosition = false;
    }

    // Слідкувати за позицією
    public void FollowPosition(Vector3 position)
    {
        targetPosition = position;
        targetPlayer = null;
        targetTransform = null;
        followingPosition = true;
    }

    // Повернутися до локального гравця
    public void ReturnToPlayer()
    {
        FindLocalPlayer();
        if (targetPlayer != null)
        {
            FollowPlayer(targetPlayer);
        }
    }

    // Миттєве переміщення до цілі
    public void SnapToTarget()
    {
        Vector3 targetPos;
        if (followingPosition)
            targetPos = targetPosition;
        else if (targetTransform != null)
            targetPos = targetTransform.position;
        else if (targetPlayer != null)
            targetPos = targetPlayer.transform.position;
        else
            return;

        transform.position = targetPos + offset;
    }
    #endregion

    // Публічні методи для налаштування камери
    public void SetTarget(Player player)
    {
        targetPlayer = player;
    }

    public void SetBounds(Vector2 bounds)
    {
        mapBounds = bounds;
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    public void SetSmoothTime(float time)
    {
        smoothTime = time;
    }

    // Допоміжний метод для конвертації позиції світу в позицію на екрані
    public Vector2 WorldToScreenPoint(Vector3 worldPosition)
    {
        return mainCamera.WorldToScreenPoint(worldPosition);
    }

    // Допоміжний метод для конвертації позиції екрану в позицію в світі
    public Vector3 ScreenToWorldPoint(Vector3 screenPosition)
    {
        return mainCamera.ScreenToWorldPoint(screenPosition);
    }
}
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    [Header("Prefabs")]
    [SerializeField] private PlayerEntity playerPrefab;
    [SerializeField] private BotEntity botPrefab;
    [SerializeField] private MobEntity mobPrefab;
    [SerializeField] private ResourceNode resourcePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int initialBotsCount = 5;
    [SerializeField] private int initialMobsCount = 3;
    [SerializeField] private int initialResourcesCount = 10;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Підписуємося на події підключення/відключення клієнтів
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if (IsServer)
        {
            SpawnInitialEntities();
        }
    }

    public override void OnNetworkDespawn()
    {
        // Відписуємося від подій
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        base.OnNetworkDespawn();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        // Спавнимо гравця для нового клієнта
        Vector2 spawnPosition = Random.insideUnitCircle * 5f; // Випадкова позиція в радіусі 5 одиниць
        SpawnPlayer(spawnPosition, clientId);

        Debug.Log($"Client {clientId} connected. Spawning player...");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;
        Debug.Log($"Client {clientId} disconnected");
    }

    private void SpawnInitialEntities()
    {
        // Спавн ботів
        for (int i = 0; i < initialBotsCount; i++)
        {
            Vector2 randomPosition = Random.insideUnitCircle * 10f;
            SpawnBot(randomPosition);
        }

        // Спавн мобів
        for (int i = 0; i < initialMobsCount; i++)
        {
            Vector2 randomPosition = Random.insideUnitCircle * 15f;
            SpawnMob(randomPosition);
        }

        // Спавн ресурсів
        for (int i = 0; i < initialResourcesCount; i++)
        {
            Vector2 randomPosition = Random.insideUnitCircle * 20f;
            SpawnResource(randomPosition);
        }
    }

    public void SpawnPlayer(Vector2 position, ulong clientId)
    {
        if (!IsServer) return;

        var player = Instantiate(playerPrefab, position, Quaternion.identity);
        var networkObject = player.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(clientId);

        Debug.Log($"Spawning player for client {clientId}");
    }

    public void SpawnBot(Vector2 position)
    {
        if (!IsServer) return;

        var bot = Instantiate(botPrefab, position, Quaternion.identity);
        bot.NetworkObject.Spawn();
    }

    public void SpawnMob(Vector2 position)
    {
        if (!IsServer) return;

        var mob = Instantiate(mobPrefab, position, Quaternion.identity);
        mob.NetworkObject.Spawn();
    }

    public void SpawnResource(Vector2 position)
    {
        if (!IsServer) return;

        Debug.Log($"[Server] Spawning resource at {position}"); // DEBUG
        var resource = Instantiate(resourcePrefab, position, Quaternion.identity);
        resource.NetworkObject.Spawn();
    }
}
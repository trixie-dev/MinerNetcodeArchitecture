using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectManager : NetworkBehaviour
{
    private static ObjectManager instance;
    public static ObjectManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ObjectManager>();
            return instance;
        }
    }

    // Списки різних типів об'єктів
    private readonly List<PlayerEntity> players = new();
    private readonly List<BotEntity> bots = new();
    private readonly List<MobEntity> mobs = new();
    private readonly List<ResourceNode> resources = new();

    // Публічні властивості для доступу до списків
    public IReadOnlyList<PlayerEntity> Players => players;
    public IReadOnlyList<BotEntity> Bots => bots;
    public IReadOnlyList<MobEntity> Mobs => mobs;
    public IReadOnlyList<ResourceNode> Resources => resources;

    private void Awake()
    {
        Debug.Log($"[{(IsServer ? "Server" : "Client")}] ObjectManager Awake"); // DEBUG
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning($"[{(IsServer ? "Server" : "Client")}] Duplicate ObjectManager destroyed"); // DEBUG
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"[{(IsServer ? "Server" : "Client")}] ObjectManager spawned"); // DEBUG
    }

    #region Entity Registration

    public void RegisterPlayer(PlayerEntity player)
    {
        if (!players.Contains(player))
            players.Add(player);
    }

    public void UnregisterPlayer(PlayerEntity player)
    {
        players.Remove(player);
    }

    public void RegisterBot(BotEntity bot)
    {
        if (!bots.Contains(bot))
            bots.Add(bot);
    }

    public void UnregisterBot(BotEntity bot)
    {
        bots.Remove(bot);
    }

    public void RegisterMob(MobEntity mob)
    {
        if (mob == null) return;

        Debug.Log($"[{(IsServer ? "Server" : "Client")}] Registering mob. Current count: {mobs.Count}"); // DEBUG
        if (!mobs.Contains(mob))
        {
            mobs.Add(mob);
            Debug.Log($"[{(IsServer ? "Server" : "Client")}] Mob registered. New count: {mobs.Count}"); // DEBUG
        }
    }

    public void UnregisterMob(MobEntity mob)
    {
        if (mob == null) return;

        Debug.Log($"[{(IsServer ? "Server" : "Client")}] Unregistering mob. Current count: {mobs.Count}"); // DEBUG
        mobs.Remove(mob);
        Debug.Log($"[{(IsServer ? "Server" : "Client")}] Mob unregistered. New count: {mobs.Count}"); // DEBUG
    }

    public void RegisterResource(ResourceNode resource)
    {
        if (resource == null) return;

        Debug.Log($"[{(IsServer ? "Server" : "Client")}] Registering resource. Current count: {resources.Count}"); // DEBUG
        if (!resources.Contains(resource))
        {
            resources.Add(resource);
            Debug.Log($"[{(IsServer ? "Server" : "Client")}] Resource registered. New count: {resources.Count}"); // DEBUG
        }
    }

    public void UnregisterResource(ResourceNode resource)
    {
        if (resource == null) return;

        Debug.Log($"[{(IsServer ? "Server" : "Client")}] Unregistering resource. Current count: {resources.Count}"); // DEBUG
        resources.Remove(resource);
        Debug.Log($"[{(IsServer ? "Server" : "Client")}] Resource unregistered. New count: {resources.Count}"); // DEBUG
    }

    #endregion

    #region Helper Methods

    // Пошук найближчого моба до позиції
    public MobEntity GetNearestMob(Vector2 position)
    {
        Debug.Log($"[{(IsServer ? "Server" : "Client")}] GetNearestMob called. Mobs count: {mobs.Count}"); // DEBUG

        MobEntity nearest = null;
        float minDistance = float.MaxValue;

        foreach (var mob in mobs)
        {
            if (mob == null)
            {
                Debug.LogWarning($"[{(IsServer ? "Server" : "Client")}] Null mob found in list!"); // DEBUG
                continue;
            }

            float distance = Vector2.SqrMagnitude(position - mob.Position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = mob;
            }
        }

        Debug.Log($"[{(IsServer ? "Server" : "Client")}] Nearest mob found: {(nearest != null ? $"at distance {Mathf.Sqrt(minDistance)}" : "none")}"); // DEBUG
        return nearest;
    }

    // Пошук найближчого гравця до позиції
    public PlayerEntity GetNearestPlayer(Vector2 position)
    {
        float nearestDistance = float.MaxValue;
        PlayerEntity nearestPlayer = null;

        foreach (var player in players)
        {
            float distance = Vector2.Distance(position, player.Position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPlayer = player;
            }
        }

        return nearestPlayer;
    }

    // Пошук найближчого ресурсу до позиції
    public ResourceNode GetNearestResource(Vector2 position)
    {
        Debug.Log($"[{(IsServer ? "Server" : "Client")}] GetNearestResource called. Resources count: {resources.Count}"); // DEBUG

        ResourceNode nearest = null;
        float minDistance = float.MaxValue;

        foreach (var resource in resources)
        {
            if (resource == null)
            {
                Debug.LogWarning($"[{(IsServer ? "Server" : "Client")}] Null resource found in list!"); // DEBUG
                continue;
            }

            float distance = Vector2.SqrMagnitude(position - resource.Position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = resource;
            }
        }

        Debug.Log($"[{(IsServer ? "Server" : "Client")}] Nearest resource found: {(nearest != null ? $"at distance {Mathf.Sqrt(minDistance)}" : "none")}"); // DEBUG
        return nearest;
    }

    // Отримання всіх сутностей в радіусі
    public List<Entity> GetEntitiesInRadius(Vector2 position, float radius)
    {
        List<Entity> result = new List<Entity>();
        float sqrRadius = radius * radius;

        foreach (var player in players)
        {
            if (Vector2.SqrMagnitude(position - player.Position) <= sqrRadius)
                result.Add(player);
        }

        foreach (var bot in bots)
        {
            if (Vector2.SqrMagnitude(position - bot.Position) <= sqrRadius)
                result.Add(bot);
        }

        foreach (var mob in mobs)
        {
            if (Vector2.SqrMagnitude(position - mob.Position) <= sqrRadius)
                result.Add(mob);
        }

        return result;
    }

    #endregion
}
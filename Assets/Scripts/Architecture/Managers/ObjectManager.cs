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
        if (!mobs.Contains(mob))
            mobs.Add(mob);
    }

    public void UnregisterMob(MobEntity mob)
    {
        mobs.Remove(mob);
    }

    public void RegisterResource(ResourceNode resource)
    {
        if (!resources.Contains(resource))
            resources.Add(resource);
    }

    public void UnregisterResource(ResourceNode resource)
    {
        resources.Remove(resource);
    }

    #endregion

    #region Helper Methods

    // Пошук найближчого моба до позиції
    public MobEntity GetNearestMob(Vector2 position)
    {
        float nearestDistance = float.MaxValue;
        MobEntity nearestMob = null;

        foreach (var mob in mobs)
        {
            float distance = Vector2.Distance(position, mob.Position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestMob = mob;
            }
        }

        return nearestMob;
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
        float nearestDistance = float.MaxValue;
        ResourceNode nearestResource = null;

        foreach (var resource in resources)
        {
            float distance = Vector2.Distance(position, resource.Position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestResource = resource;
            }
        }

        return nearestResource;
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
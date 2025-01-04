using System;
using Unity.Netcode;
using UnityEngine;

public class Pickaxe : Equipment
{
    public float Damage { get; private set; }

    public Pickaxe(int id, string name, string description, float damage)
    {
        Id = id;
        Name = name;
        Description = description;
        Damage = damage;
    }

    public override string GetStatsDescription()
    {
        return $"{base.GetStatsDescription()}\nDamage: {Damage}";
    }

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out float damage);
            Damage = damage;
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(Damage);
        }
    }
}
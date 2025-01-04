using System;
using Unity.Netcode;
using UnityEngine;

public class Armor : Equipment
{
    public float Defense { get; private set; }

    public Armor(int id, string name, string description, float defense)
    {
        Id = id;
        Name = name;
        Description = description;
        Defense = defense;
    }

    public override string GetStatsDescription()
    {
        return $"{base.GetStatsDescription()}\nDefense: {Defense}";
    }

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out float defense);
            Defense = defense;
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(Defense);
        }
    }
}
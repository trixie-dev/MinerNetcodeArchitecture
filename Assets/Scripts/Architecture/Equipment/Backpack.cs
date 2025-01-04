using System;
using Unity.Netcode;
using UnityEngine;

public class Backpack : Equipment
{
    public float MaxCapacity { get; private set; }

    public Backpack(int id, string name, string description, float maxCapacity)
    {
        Id = id;
        Name = name;
        Description = description;
        MaxCapacity = maxCapacity;
    }

    public override string GetStatsDescription()
    {
        return $"{base.GetStatsDescription()}\nCapacity: {MaxCapacity}";
    }

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out float capacity);
            MaxCapacity = capacity;
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(MaxCapacity);
        }
    }
}
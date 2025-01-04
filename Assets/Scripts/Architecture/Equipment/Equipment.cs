using System;
using Unity.Netcode;
using UnityEngine;

public abstract class Equipment : INetworkSerializable
{
    public int Id { get; protected set; }
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public Sprite Icon { get; protected set; }

    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out int id);
            Id = id;
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(Id);
        }
    }

    public virtual string GetStatsDescription()
    {
        return Description;
    }
}
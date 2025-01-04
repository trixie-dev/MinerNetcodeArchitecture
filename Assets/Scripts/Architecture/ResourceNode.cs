using Unity.Netcode;
using UnityEngine;

public class ResourceNode : NetworkBehaviour
{
    [Header("Resource Properties")]
    [SerializeField] private float maxAmount = 100f;

    private NetworkVariable<float> currentAmount = new NetworkVariable<float>();

    public Vector2 Position => transform.position;
    public float CurrentAmount => currentAmount.Value;
    public float MaxAmount => maxAmount;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            currentAmount.Value = maxAmount;
            ObjectManager.Instance.RegisterResource(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            ObjectManager.Instance.UnregisterResource(this);
        }
        base.OnNetworkDespawn();
    }

    public bool TryHarvest(float amount)
    {
        if (!IsServer) return false;

        if (currentAmount.Value >= amount)
        {
            currentAmount.Value -= amount;

            // Якщо ресурс вичерпано - знищуємо його
            if (currentAmount.Value <= 0)
            {
                NetworkObject.Despawn(true);
            }

            return true;
        }
        return false;
    }
}
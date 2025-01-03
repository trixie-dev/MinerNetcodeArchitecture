using Unity.Netcode;
using UnityEngine;

public class ResourceNode : NetworkBehaviour
{
    [Header("Resource Properties")]
    [SerializeField] private float maxAmount = 100f;
    [SerializeField] private float regenerationRate = 1f;

    private NetworkVariable<float> currentAmount = new NetworkVariable<float>();

    public Vector2 Position => transform.position;
    public float CurrentAmount => currentAmount.Value;

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

    private void Update()
    {
        if (IsServer)
        {
            RegenerateResource();
        }
    }

    private void RegenerateResource()
    {
        if (currentAmount.Value < maxAmount)
        {
            currentAmount.Value = Mathf.Min(maxAmount,
                currentAmount.Value + regenerationRate * Time.deltaTime);
        }
    }

    public bool TryHarvest(float amount)
    {
        if (!IsServer) return false;

        if (currentAmount.Value >= amount)
        {
            currentAmount.Value -= amount;
            return true;
        }
        return false;
    }
}
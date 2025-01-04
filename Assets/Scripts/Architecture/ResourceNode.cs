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

    public delegate void OnHarvestSuccess(float amount);

    public bool TryHarvest(float amount, OnHarvestSuccess onSuccess = null)
    {
        if (!IsServer)
        {
            RequestHarvestServerRpc(amount, new ServerRpcParams());
            return true;
        }

        if (currentAmount.Value >= amount)
        {
            currentAmount.Value -= amount;
            DebugUtil.LogResources($"Resource harvested: {amount:F1}, Remaining: {currentAmount.Value:F1}");

            if (currentAmount.Value <= 0)
            {
                DebugUtil.LogResources("Resource depleted, despawning");
                NetworkObject.Despawn(true);
            }

            onSuccess?.Invoke(amount);
            NotifyHarvestSuccessClientRpc(amount);
            return true;
        }

        NotifyHarvestFailedClientRpc();
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestHarvestServerRpc(float amount, ServerRpcParams rpcParams = default)
    {
        DebugUtil.LogNetwork($"Harvest request received from client {rpcParams.Receive.SenderClientId} for amount {amount}");

        if (currentAmount.Value >= amount)
        {
            currentAmount.Value -= amount;
            DebugUtil.LogResources($"Resource harvested: {amount:F1}, Remaining: {currentAmount.Value:F1}");

            if (currentAmount.Value <= 0)
            {
                DebugUtil.LogResources("Resource depleted, despawning");
                NetworkObject.Despawn(true);
            }

            bool resourceAdded = false;
            var players = Object.FindObjectsOfType<Player>();
            foreach (var player in players)
            {
                if (player.OwnerClientId == rpcParams.Receive.SenderClientId)
                {
                    player.AddResource(amount);
                    resourceAdded = true;

                    NotifyHarvestSuccessClientRpc(amount, new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new[] { rpcParams.Receive.SenderClientId }
                        }
                    });
                    break;
                }
            }

            if (!resourceAdded)
            {
                var bots = Object.FindObjectsOfType<BotEntity>();
                foreach (var bot in bots)
                {
                    if (bot.NetworkObjectId == rpcParams.Receive.SenderClientId)
                    {
                        bot.AddResource(amount);
                        DebugUtil.LogAI($"Bot {bot.NetworkObjectId} harvested {amount} resources");
                        break;
                    }
                }
            }
        }
        else
        {
            NotifyHarvestFailedClientRpc();
        }
    }

    [ClientRpc]
    private void NotifyHarvestSuccessClientRpc(float amount, ClientRpcParams clientRpcParams = default)
    {
        DebugUtil.LogResources($"Successfully harvested {amount:F1} resources");
    }

    [ClientRpc]
    private void NotifyHarvestFailedClientRpc()
    {
        DebugUtil.LogResources("Failed to harvest resource", LogType.Warning);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        DebugUtil.LogNetwork($"ResourceNode spawned on {DebugUtil.FormatNetworkSide(IsServer)}");
        ObjectManager.Instance.RegisterResource(this);

        if (IsServer)
        {
            currentAmount.Value = maxAmount;
        }
    }

    public override void OnNetworkDespawn()
    {
        DebugUtil.LogNetwork($"ResourceNode despawned on {DebugUtil.FormatNetworkSide(IsServer)}");
        ObjectManager.Instance.UnregisterResource(this);
        base.OnNetworkDespawn();
    }
}
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EffectsComponent : NetworkBehaviour
{
    private readonly List<Effect> activeEffects = new();
    private Entity owner;

    private void Awake()
    {
        owner = GetComponent<Entity>();
    }

    private void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            effect.Update(Time.deltaTime);

            if (!effect.IsActive)
            {
                activeEffects.RemoveAt(i);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddEffectServerRpc(EffectType effectType, float duration, ServerRpcParams rpcParams = default)
    {
        Effect effect = null;

        switch (effectType)
        {
            case EffectType.SpeedBoost:
                effect = new SpeedBoostEffect(owner, duration);
                break;
            case EffectType.Stun:
                effect = new StunEffect(owner, duration);
                break;
        }

        if (effect != null)
        {
            effect.Apply();
            activeEffects.Add(effect);

            // Відправляємо візуальні ефекти всім клієнтам
            NotifyEffectAddedClientRpc(effectType, duration);

            // Логуємо для відлагодження
            string targetType = owner.GetType().Name;
            ulong targetId = owner.NetworkObjectId;
            DebugUtil.LogGameplay($"Effect {effectType} applied to {targetType} (ID: {targetId})");
        }
    }

    [ClientRpc]
    private void NotifyEffectAddedClientRpc(EffectType effectType, float duration)
    {
        // Візуальні ефекти на клієнті
        switch (effectType)
        {
            case EffectType.SpeedBoost:
                // Додати візуальний ефект прискорення
                DebugUtil.LogGameplay($"Speed boost visual effect on {owner.name}");
                break;
            case EffectType.Stun:
                // Додати візуальний ефект оглушення
                DebugUtil.LogGameplay($"Stun visual effect on {owner.name}");
                break;
        }
    }
}

public enum EffectType
{
    SpeedBoost,
    Stun
}
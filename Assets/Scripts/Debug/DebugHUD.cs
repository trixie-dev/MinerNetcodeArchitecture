using UnityEngine;
using Unity.Netcode;

public class DebugHUD : MonoBehaviour
{
    private Player localPlayer;
    private Vector2 scrollPosition;
    private bool showDebugInfo = true;
    private float updateInterval = 0.5f;
    private float nextUpdateTime;

    private string debugInfo = "";

    private void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateDebugInfo();
            nextUpdateTime = Time.time + updateInterval;
        }

        // Перемикання відображення дебаг інформації
        if (Input.GetKeyDown(KeyCode.F3))
        {
            showDebugInfo = !showDebugInfo;
        }
    }

    private void UpdateDebugInfo()
    {
        if (localPlayer == null)
        {
            // Шукаємо локального гравця
            var players = FindObjectsOfType<Player>();
            foreach (var player in players)
            {
                if (player.IsOwner)
                {
                    localPlayer = player;
                    break;
                }
            }
        }

        if (localPlayer != null)
        {
            var stats = localPlayer.GetComponent<EntityStatsComponent>();
            var backpack = localPlayer.GetComponent<BackpackComponent>();
            var networkObject = localPlayer.GetComponent<NetworkObject>();
            var attackComponent = localPlayer.GetComponent<AttackComponent>();

            debugInfo = $"Player Debug Info:\n";
            debugInfo += $"Network State: {(NetworkManager.Singleton.IsServer ? "Server" : "Client")}\n";
            debugInfo += $"ClientId: {networkObject.OwnerClientId}\n";
            debugInfo += $"Position: {localPlayer.Position:F1}\n";

            if (stats != null)
            {
                debugInfo += "\nStats:\n";
                debugInfo += $"Health: {stats.Health:F1}\n";
                debugInfo += $"Stamina: {stats.Stamina:F1}\n";
                debugInfo += $"Damage: {stats.Damage:F1}\n";
                debugInfo += $"Armor: {stats.Armor:F1}\n";
                debugInfo += $"Attack Speed: {stats.AttackSpeed:F1}\n";
                debugInfo += $"Move Speed: {stats.MoveSpeed:F1}\n";
            }

            if (backpack != null)
            {
                debugInfo += "\nBackpack:\n";
                debugInfo += $"Capacity: {backpack.CurrentCapacity:F1}/{backpack.MaxCapacity:F1}\n";
                debugInfo += $"Resources: {backpack.ResourceAmount:F1}\n";
                debugInfo += $"Is Full: {backpack.IsFull}\n";
                debugInfo += $"Is Broken: {backpack.IsBroken}\n";
            }

            if (attackComponent != null)
            {
                debugInfo += $"\nAttack Info:\n";
                debugInfo += $"Attack Range: {attackComponent.AttackRange:F1}m\n";
                debugInfo += $"Can Attack: {attackComponent.CanAttack()}\n";
            }

            // Додаємо інформацію про найближчі об'єкти
            var nearestMob = ObjectManager.Instance.GetNearestMob(localPlayer.Position);
            if (nearestMob != null)
            {
                float distance = Vector2.Distance(localPlayer.Position, nearestMob.Position);
                bool inRange = attackComponent != null && distance <= attackComponent.AttackRange;
                debugInfo += $"\nNearest Mob:\n";
                debugInfo += $"Distance: {distance:F1}m\n";
                debugInfo += $"In Attack Range: {inRange}\n";
                if (nearestMob.TryGetComponent<EntityStatsComponent>(out var mobStats))
                {
                    debugInfo += $"Health: {mobStats.Health:F1}\n";
                }
            }

            var nearestResource = ObjectManager.Instance.GetNearestResource(localPlayer.Position);
            if (nearestResource != null)
            {
                float distance = Vector2.Distance(localPlayer.Position, nearestResource.Position);
                debugInfo += $"\nNearest Resource:\n";
                debugInfo += $"Distance: {distance:F1}m\n";
                debugInfo += $"Amount: {nearestResource.CurrentAmount:F1}\n";
            }
        }
        else
        {
            debugInfo = "No local player found";
        }
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        float width = 250;
        float height = 400;
        float margin = 10;

        Rect windowRect = new Rect(
            Screen.width - width - margin,
            margin,
            width,
            height
        );

        GUILayout.BeginArea(windowRect);
        GUI.Box(new Rect(0, 0, width, height), "");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.Label(debugInfo);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
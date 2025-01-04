using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class EquipmentComponent : NetworkBehaviour
{
    private NetworkVariable<int> netPickaxeId = new NetworkVariable<int>();
    private NetworkVariable<int> netArmorId = new NetworkVariable<int>();
    private NetworkVariable<int> netBackpackId = new NetworkVariable<int>();

    public Pickaxe CurrentPickaxe => EquipmentFactory.GetEquipmentById(netPickaxeId.Value) as Pickaxe;
    public Armor CurrentArmor => EquipmentFactory.GetEquipmentById(netArmorId.Value) as Armor;
    public Backpack CurrentBackpack => EquipmentFactory.GetEquipmentById(netBackpackId.Value) as Backpack;

    public event UnityAction<Equipment> OnEquipmentChanged;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // Встановлюємо базове спорядження
            netPickaxeId.Value = EquipmentFactory.WoodenPickaxe.Id;
            netArmorId.Value = EquipmentFactory.LeatherArmor.Id;
            netBackpackId.Value = EquipmentFactory.SmallBackpack.Id;
        }

        netPickaxeId.OnValueChanged += (_, newValue) => OnEquipmentChanged?.Invoke(EquipmentFactory.GetEquipmentById(newValue));
        netArmorId.OnValueChanged += (_, newValue) => OnEquipmentChanged?.Invoke(EquipmentFactory.GetEquipmentById(newValue));
        netBackpackId.OnValueChanged += (_, newValue) => OnEquipmentChanged?.Invoke(EquipmentFactory.GetEquipmentById(newValue));
    }

    [ServerRpc]
    public void EquipItemServerRpc(int itemId)
    {
        var equipment = EquipmentFactory.GetEquipmentById(itemId);
        if (equipment == null) return;

        if (equipment is Pickaxe)
            netPickaxeId.Value = itemId;
        else if (equipment is Armor)
            netArmorId.Value = itemId;
        else if (equipment is Backpack)
            netBackpackId.Value = itemId;
    }
}
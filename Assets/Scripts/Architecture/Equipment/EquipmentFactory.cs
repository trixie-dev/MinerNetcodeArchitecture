using System.Collections.Generic;
using UnityEngine;

public static class EquipmentFactory
{
    // ID Ranges
    private const int PICKAXE_ID_START = 1000;
    private const int ARMOR_ID_START = 2000;
    private const int BACKPACK_ID_START = 3000;

    // Pickaxes
    public static readonly Pickaxe WoodenPickaxe = new Pickaxe(
        PICKAXE_ID_START + 1,
        "Wooden Pickaxe",
        "A basic wooden pickaxe",
        10f
    );

    public static readonly Pickaxe IronPickaxe = new Pickaxe(
        PICKAXE_ID_START + 2,
        "Iron Pickaxe",
        "Standard iron mining tool",
        20f
    );

    public static readonly Pickaxe DiamondPickaxe = new Pickaxe(
        PICKAXE_ID_START + 3,
        "Diamond Pickaxe",
        "High-quality diamond mining tool",
        35f
    );

    // Armors
    public static readonly Armor LeatherArmor = new Armor(
        ARMOR_ID_START + 1,
        "Leather Armor",
        "Basic leather protection",
        5f
    );

    public static readonly Armor ChainArmor = new Armor(
        ARMOR_ID_START + 2,
        "Chain Armor",
        "Medium protection chainmail",
        10f
    );

    public static readonly Armor PlateArmor = new Armor(
        ARMOR_ID_START + 3,
        "Plate Armor",
        "Heavy plate armor",
        20f
    );

    // Backpacks
    public static readonly Backpack SmallBackpack = new Backpack(
        BACKPACK_ID_START + 1,
        "Small Backpack",
        "Basic storage bag",
        50f
    );

    public static readonly Backpack MediumBackpack = new Backpack(
        BACKPACK_ID_START + 2,
        "Medium Backpack",
        "Standard adventurer's backpack",
        100f
    );

    public static readonly Backpack LargeBackpack = new Backpack(
        BACKPACK_ID_START + 3,
        "Large Backpack",
        "Spacious expedition pack",
        200f
    );

    // Списки всього спорядження за типами
    public static readonly List<Pickaxe> AllPickaxes = new List<Pickaxe>
    {
        WoodenPickaxe,
        IronPickaxe,
        DiamondPickaxe
    };

    public static readonly List<Armor> AllArmors = new List<Armor>
    {
        LeatherArmor,
        ChainArmor,
        PlateArmor
    };

    public static readonly List<Backpack> AllBackpacks = new List<Backpack>
    {
        SmallBackpack,
        MediumBackpack,
        LargeBackpack
    };

    // Метод для отримання спорядження за ID
    public static Equipment GetEquipmentById(int id)
    {
        if (id >= PICKAXE_ID_START && id < ARMOR_ID_START)
        {
            return AllPickaxes.Find(p => p.Id == id);
        }
        else if (id >= ARMOR_ID_START && id < BACKPACK_ID_START)
        {
            return AllArmors.Find(a => a.Id == id);
        }
        else if (id >= BACKPACK_ID_START)
        {
            return AllBackpacks.Find(b => b.Id == id);
        }
        return null;
    }
}
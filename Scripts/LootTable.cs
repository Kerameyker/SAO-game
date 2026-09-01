using UnityEngine;

/// <summary>
/// Генерує випадкові предмети.
/// RollItem() - для звичайних мобів: Легендарні й Міфічні речі НЕ випадають узагалі,
/// навіть Епічні - вкрай рідкісний випадок. RollBossItem() - для босів: гарантовано
/// мінімум Рідкісний, з реальними шансами на Епічний/Легендарний/Міфічний.
/// </summary>
public static class LootTable
{
    static readonly string[] weaponNames = { "Меч", "Клинок", "Сокира", "Спис", "Кинджал" };
    static readonly string[] armorNames = { "Кіраса", "Наплічники", "Обладунок" };
    static readonly string[] helmetNames = { "Шолом", "Капюшон", "Обруч" };
    static readonly string[] cloakNames = { "Плащ", "Мантія", "Накидка" };
    static readonly string[] bootsNames = { "Чоботи", "Сандалі", "Поножі" };

    public static ItemData RollItem(int mobLevel)
    {
        return BuildItem(RollRarity(mobLevel), mobLevel);
    }

    public static ItemData RollBossItem(int bossLevel)
    {
        return BuildItem(RollBossRarity(), bossLevel);
    }

    static ItemData BuildItem(ItemRarity rarity, int level)
    {
        ItemType type = (ItemType)Random.Range(0, 5);
        float rarityMul = GetRarityMultiplier(rarity);
        float baseStat = (type == ItemType.Weapon) ? 3f : (type == ItemType.Armor) ? 2f : 1f;
        float stat = Mathf.Round(baseStat * rarityMul * (1f + level * 0.15f));

        string name = PickName(type);
        string statLabel = (type == ItemType.Weapon) ? "Атака"
            : (type == ItemType.Cloak || type == ItemType.Boots) ? "Швидкість"
            : "Захист";

        return new ItemData
        {
            itemName = ItemData.GetRarityLabel(rarity) + " " + name,
            type = type,
            rarity = rarity,
            statBonus = stat,
            statLabel = statLabel
        };
    }

    static string PickName(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon: return weaponNames[Random.Range(0, weaponNames.Length)];
            case ItemType.Armor: return armorNames[Random.Range(0, armorNames.Length)];
            case ItemType.Helmet: return helmetNames[Random.Range(0, helmetNames.Length)];
            case ItemType.Cloak: return cloakNames[Random.Range(0, cloakNames.Length)];
            case ItemType.Boots: return bootsNames[Random.Range(0, bootsNames.Length)];
        }
        return "Предмет";
    }

    /// <summary>Звичайні моби: Легендарні/Міфічні речі не випадають узагалі; Епічні - виняток, не правило.</summary>
    static ItemRarity RollRarity(int mobLevel)
    {
        float bonus = mobLevel * 1.2f; // рівень моба впливає значно слабше, ніж раніше
        float roll = Random.Range(0f, 100f) - bonus;
        if (roll < 0.4f) return ItemRarity.Epic;      // ~0.4% - дуже рідкісний виняток
        if (roll < 3f) return ItemRarity.Rare;        // ~3% - рідко
        if (roll < 18f) return ItemRarity.Uncommon;   // ~18% - час від часу
        return ItemRarity.Common;                      // решта - здебільшого
    }

    /// <summary>Боси: гарантовано мінімум Рідкісний, з реальними шансами на топові рівні рідкості.</summary>
    static ItemRarity RollBossRarity()
    {
        float roll = Random.Range(0f, 100f);
        if (roll < 8f) return ItemRarity.Mythic;
        if (roll < 35f) return ItemRarity.Legendary;
        if (roll < 70f) return ItemRarity.Epic;
        return ItemRarity.Rare;
    }

    static float GetRarityMultiplier(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return 1f;
            case ItemRarity.Uncommon: return 1.4f;
            case ItemRarity.Rare: return 2f;
            case ItemRarity.Epic: return 2.8f;
            case ItemRarity.Legendary: return 4f;
            case ItemRarity.Mythic: return 5.5f;
        }
        return 1f;
    }
}

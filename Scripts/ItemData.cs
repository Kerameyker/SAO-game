using UnityEngine;

public enum ItemType { Weapon, Armor, Helmet, Cloak, Boots }
public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary, Mythic }

/// <summary>
/// Опис одного предмета спорядження. Не MonoBehaviour - просто дані.
/// </summary>
[System.Serializable]
public class ItemData
{
    public string itemName;
    public ItemType type;
    public ItemRarity rarity;
    public float statBonus;
    public string statLabel; // "Атака", "Захист" або "Швидкість"

    public static Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return new Color(0.72f, 0.67f, 0.55f);
            case ItemRarity.Uncommon: return new Color(0.43f, 0.68f, 0.35f);
            case ItemRarity.Rare: return new Color(0.29f, 0.56f, 0.79f);
            case ItemRarity.Epic: return new Color(0.63f, 0.4f, 0.82f);
            case ItemRarity.Legendary: return new Color(0.88f, 0.66f, 0.24f);
            case ItemRarity.Mythic: return new Color(0.85f, 0.15f, 0.18f);
        }
        return Color.white;
    }

    public static string GetRarityLabel(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "Звичайний";
            case ItemRarity.Uncommon: return "Незвичайний";
            case ItemRarity.Rare: return "Рідкісний";
            case ItemRarity.Epic: return "Епічний";
            case ItemRarity.Legendary: return "Легендарний";
            case ItemRarity.Mythic: return "Міфічний";
        }
        return "";
    }
}

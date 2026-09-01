using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Повісь на Player. Тримає сумку (List) і екіпіровані предмети (по одному на тип слота).
/// </summary>
public class Inventory : MonoBehaviour
{
    public List<ItemData> bag = new List<ItemData>();
    [NonSerialized] public Dictionary<ItemType, ItemData> equipped = new Dictionary<ItemType, ItemData>();

    public event Action onInventoryChanged;

    public void AddItem(ItemData item)
    {
        bag.Add(item);
        onInventoryChanged?.Invoke();
    }

    public void Equip(ItemData item)
    {
        if (!bag.Contains(item)) return;
        if (equipped.ContainsKey(item.type))
        {
            bag.Add(equipped[item.type]); // стара річ повертається в сумку
        }
        equipped[item.type] = item;
        bag.Remove(item);
        onInventoryChanged?.Invoke();
    }

    public void Unequip(ItemType type)
    {
        if (!equipped.ContainsKey(type)) return;
        bag.Add(equipped[type]);
        equipped.Remove(type);
        onInventoryChanged?.Invoke();
    }

    public float GetBonus(ItemType type)
    {
        return equipped.ContainsKey(type) ? equipped[type].statBonus : 0f;
    }

    public float TotalAttackBonus => GetBonus(ItemType.Weapon);
    public float TotalDefenseBonus => GetBonus(ItemType.Armor) + GetBonus(ItemType.Helmet);
    public float TotalSpeedBonus => GetBonus(ItemType.Cloak) + GetBonus(ItemType.Boots);
}

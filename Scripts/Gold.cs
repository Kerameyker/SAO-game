using UnityEngine;
using System;

/// <summary>
/// Проста валюта гравця. Повісь на Player. Моби видають золото при вбивстві,
/// а в майбутньому магазині можна буде списувати його через TrySpend().
/// </summary>
public class Gold : MonoBehaviour
{
    public int amount = 0;
    public event Action onGoldChanged;

    public void Add(int value)
    {
        if (value <= 0) return;
        amount += value;
        onGoldChanged?.Invoke();
    }

    public bool TrySpend(int cost)
    {
        if (amount < cost) return false;
        amount -= cost;
        onGoldChanged?.Invoke();
        return true;
    }
}

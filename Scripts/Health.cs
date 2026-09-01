using UnityEngine;
using System;

/// <summary>
/// Універсальний компонент здоров'я. Вішай і на гравця, і на мобів.
/// </summary>
public class Health : MonoBehaviour
{
    public float maxHealth = 30f;
    public float currentHealth;
    public float regenPerSecond = 0.8f; // невелика пасивна регенерація - не занадто сильна
    public event Action onDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (currentHealth > 0f && currentHealth < maxHealth)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + regenPerSecond * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            onDeath?.Invoke();
        }
    }

    public bool IsDead => currentHealth <= 0f;

    public void Revive()
    {
        currentHealth = maxHealth;
    }
}

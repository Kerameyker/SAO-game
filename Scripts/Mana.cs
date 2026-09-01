using UnityEngine;

/// <summary>
/// Проста мана з пасивною регенерацією. Повісь на Player поруч із Health.
/// </summary>
public class Mana : MonoBehaviour
{
    public float maxMana = 50f;
    public float currentMana;
    public float regenPerSecond = 2f;

    void Awake()
    {
        currentMana = maxMana;
    }

    void Update()
    {
        if (currentMana < maxMana)
        {
            currentMana = Mathf.Min(maxMana, currentMana + regenPerSecond * Time.deltaTime);
        }
    }

    public bool TrySpend(float amount)
    {
        if (currentMana < amount) return false;
        currentMana -= amount;
        return true;
    }
}

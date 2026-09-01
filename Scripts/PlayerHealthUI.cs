using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Повісь на будь-який об'єкт UI (наприклад, фон шкали HP). Автоматично знайде
/// здоров'я гравця, якщо поле targetHealth лишити порожнім.
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    public Health targetHealth;
    public Slider slider;
    public Text label;

    void Start()
    {
        if (targetHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) targetHealth = player.GetComponent<Health>();
        }
    }

    void Update()
    {
        if (targetHealth == null || slider == null) return;
        slider.maxValue = targetHealth.maxHealth;
        slider.value = targetHealth.currentHealth;
        if (label != null)
            label.text = Mathf.CeilToInt(targetHealth.currentHealth) + " / " + Mathf.CeilToInt(targetHealth.maxHealth);
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Повісь на UI-об'єкт. Автоматично знайде PlayerStats гравця, якщо targetStats порожній.
/// </summary>
public class LevelUI : MonoBehaviour
{
    public PlayerStats targetStats;
    public Slider xpSlider;
    public Text levelLabel;
    public Text xpLabel;

    void Start()
    {
        if (targetStats == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) targetStats = player.GetComponent<PlayerStats>();
        }
    }

    void Update()
    {
        if (targetStats == null) return;
        if (xpSlider != null)
        {
            xpSlider.maxValue = targetStats.XpToNext;
            xpSlider.value = targetStats.currentXP;
        }
        if (levelLabel != null)
        {
            levelLabel.text = "Рівень " + targetStats.level + " · Очки: " + targetStats.skillPoints;
        }
        if (xpLabel != null)
        {
            xpLabel.text = Mathf.FloorToInt(targetStats.currentXP) + " / " + Mathf.CeilToInt(targetStats.XpToNext);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ManaUI : MonoBehaviour
{
    public Mana targetMana;
    public Slider slider;
    public Text label;

    void Start()
    {
        if (targetMana == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) targetMana = player.GetComponent<Mana>();
        }
    }

    void Update()
    {
        if (targetMana == null || slider == null) return;
        slider.maxValue = targetMana.maxMana;
        slider.value = targetMana.currentMana;
        if (label != null)
            label.text = Mathf.CeilToInt(targetMana.currentMana) + " / " + Mathf.CeilToInt(targetMana.maxMana);
    }
}

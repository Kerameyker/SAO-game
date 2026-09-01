using UnityEngine;
using UnityEngine.UI;

public class GoldUI : MonoBehaviour
{
    public Gold targetGold;
    public Text label;

    void Start()
    {
        if (targetGold == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) targetGold = player.GetComponent<Gold>();
        }
    }

    void Update()
    {
        if (targetGold == null || label == null) return;
        label.text = "Золото: " + targetGold.amount;
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Живе на завжди-активному об'єкті (Canvas), а не на самій панелі, що ховається -
/// інакше Update() з клавішею I перестане викликатись, коли панель вимкнена.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public GameObject panelRoot;
    public Transform bagContent;
    public Text[] equippedLabels;
    public Button[] unequipButtons;

    [Header("Статистика й золото (заповнюється інструментом)")]
    public Text statsText;
    public Text goldText;
    public Text skillPointsText;

    private PlayerStats stats;
    private PlayerCombat combat;
    private PlayerSkills skills;
    private Gold gold;
    private Health health;
    private Mana mana;

    static readonly ItemType[] slotOrder = { ItemType.Weapon, ItemType.Armor, ItemType.Helmet, ItemType.Cloak, ItemType.Boots };
    static readonly string[] slotLabels = { "Зброя", "Броня", "Шолом", "Плащ", "Взуття" };

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (inventory == null) inventory = player.GetComponent<Inventory>();
            stats = player.GetComponent<PlayerStats>();
            combat = player.GetComponent<PlayerCombat>();
            skills = player.GetComponent<PlayerSkills>();
            gold = player.GetComponent<Gold>();
            health = player.GetComponent<Health>();
            mana = player.GetComponent<Mana>();
        }
        if (inventory != null) inventory.onInventoryChanged += Refresh;
        if (panelRoot != null) panelRoot.SetActive(false);
        Refresh();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && panelRoot != null)
        {
            bool nowActive = !panelRoot.activeSelf;
            panelRoot.SetActive(nowActive);
            if (nowActive) Refresh();
        }

        if (panelRoot != null && panelRoot.activeSelf)
        {
            UpdateStatsText(); // HP/мана регенерують - оновлюємо, поки панель відкрита
        }
    }

    void UpdateStatsText()
    {
        float dmg = (combat != null ? combat.attackDamage : 0f) + (inventory != null ? inventory.TotalAttackBonus : 0f);
        float def = inventory != null ? inventory.TotalDefenseBonus : 0f;
        float hpRegen = health != null ? health.regenPerSecond : 0f;
        float manaRegen = mana != null ? mana.regenPerSecond : 0f;

        if (statsText != null)
        {
            statsText.text =
                "Атака: " + dmg.ToString("0.#") + "\n" +
                "Захист: " + def.ToString("0.#") + "\n" +
                "Реген. HP: " + hpRegen.ToString("0.#") + "/сек\n" +
                "Реген. мани: " + manaRegen.ToString("0.#") + "/сек";
        }
        if (goldText != null && gold != null) goldText.text = "Золото: " + gold.amount;
        if (skillPointsText != null && stats != null) skillPointsText.text = "Очки навичок: " + stats.skillPoints;
    }

    public void Refresh()
    {
        if (inventory == null) return;

        for (int i = 0; i < slotOrder.Length; i++)
        {
            ItemData item = inventory.equipped.ContainsKey(slotOrder[i]) ? inventory.equipped[slotOrder[i]] : null;
            if (equippedLabels != null && i < equippedLabels.Length && equippedLabels[i] != null)
            {
                equippedLabels[i].text = item != null
                    ? slotLabels[i] + ": " + item.itemName + " (+" + item.statBonus + " " + item.statLabel.ToLower() + ")"
                    : slotLabels[i] + ": — порожньо —";
                equippedLabels[i].color = item != null ? ItemData.GetRarityColor(item.rarity) : Color.gray;
            }
        }

        UpdateStatsText();

        if (bagContent == null) return;
        foreach (Transform child in bagContent) Destroy(child.gameObject);

        foreach (ItemData item in inventory.bag)
        {
            GameObject row = new GameObject("Row_" + item.itemName);
            row.transform.SetParent(bagContent, false);
            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 48f;
            HorizontalLayoutGroup hl = row.AddComponent<HorizontalLayoutGroup>();
            hl.childForceExpandWidth = false;
            hl.childAlignment = TextAnchor.MiddleLeft;
            hl.spacing = 8f;
            hl.padding = new RectOffset(4, 4, 4, 4);

            // кольоровий квадрат-"іконка" за рідкістю - заміна справжнього зображення предмета
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(row.transform, false);
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = ItemData.GetRarityColor(item.rarity);
            LayoutElement iconLe = icon.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 38f;
            iconLe.preferredHeight = 38f;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(row.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = item.itemName + "\n+" + item.statBonus + " " + item.statLabel.ToLower();
            text.color = ItemData.GetRarityColor(item.rarity);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 13;
            LayoutElement textLe = textObj.AddComponent<LayoutElement>();
            textLe.preferredWidth = 170f;

            GameObject btnObj = new GameObject("EquipButton");
            btnObj.transform.SetParent(row.transform, false);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.2f, 0.2f);
            Button btn = btnObj.AddComponent<Button>();
            LayoutElement btnLe = btnObj.AddComponent<LayoutElement>();
            btnLe.preferredWidth = 92f;
            btnLe.preferredHeight = 32f;

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            Text btnText = btnTextObj.AddComponent<Text>();
            btnText.text = "Екіпірувати";
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 11;
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            ItemData capturedItem = item;
            btn.onClick.AddListener(() => inventory.Equip(capturedItem));
        }
    }

    public void UnequipSlot(int index)
    {
        if (inventory == null || index < 0 || index >= slotOrder.Length) return;
        inventory.Unequip(slotOrder[index]);
    }

    public void ClosePanel()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void UpgradeHeavy()
    {
        if (stats == null || skills == null || stats.skillPoints <= 0) return;
        stats.skillPoints--;
        skills.UpgradeHeavyAttack();
    }

    public void UpgradeWave()
    {
        if (stats == null || skills == null || stats.skillPoints <= 0) return;
        stats.skillPoints--;
        skills.UpgradeMagicWave();
    }
}

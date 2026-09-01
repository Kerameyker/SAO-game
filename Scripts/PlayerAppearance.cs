using UnityEngine;

/// <summary>
/// Повісь на Player. Слухає зміни в Inventory - коли екіпірована зброя змінюється,
/// прикріплює (чи прибирає) видиму модель меча до кістки руки персонажа.
/// </summary>
public class PlayerAppearance : MonoBehaviour
{
    private Inventory inventory;
    private Transform handBone;
    private GameObject currentWeaponVisual;
    private ItemData lastWeapon;

    void Start()
    {
        inventory = GetComponent<Inventory>();
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            handBone = FindDeepChild(anim.transform, "handr")
                ?? FindDeepChild(anim.transform, "hand_r")
                ?? FindDeepChild(anim.transform, "righthand")
                ?? FindDeepChild(anim.transform, "hand");
        }
        if (inventory != null) inventory.onInventoryChanged += UpdateWeaponVisual;
        UpdateWeaponVisual();
    }

    Transform FindDeepChild(Transform parent, string nameContainsLower)
    {
        if (parent == null) return null;
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains(nameContainsLower)) return child;
            Transform result = FindDeepChild(child, nameContainsLower);
            if (result != null) return result;
        }
        return null;
    }

    void UpdateWeaponVisual()
    {
        if (inventory == null) return;
        ItemData weapon = inventory.equipped.ContainsKey(ItemType.Weapon) ? inventory.equipped[ItemType.Weapon] : null;

        if (weapon == lastWeapon) return;
        lastWeapon = weapon;

        if (currentWeaponVisual != null) Destroy(currentWeaponVisual);
        if (weapon == null) return;
        if (handBone == null)
        {
            Debug.LogWarning("PlayerAppearance: не знайшов кістку руки на моделі персонажа - меч не відображається.");
            return;
        }

        currentWeaponVisual = BuildSwordVisual(weapon.rarity);
        currentWeaponVisual.transform.SetParent(handBone, false);
        currentWeaponVisual.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        currentWeaponVisual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
    }

    GameObject BuildSwordVisual(ItemRarity rarity)
    {
        GameObject sword = new GameObject("EquippedWeapon");

        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blade.name = "Blade";
        Destroy(blade.GetComponent<Collider>());
        blade.transform.SetParent(sword.transform, false);
        blade.transform.localScale = new Vector3(0.04f, 0.5f, 0.1f);
        blade.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        Material bladeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        bladeMat.color = ItemData.GetRarityColor(rarity);
        blade.GetComponent<Renderer>().sharedMaterial = bladeMat;

        GameObject hilt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hilt.name = "Hilt";
        Destroy(hilt.GetComponent<Collider>());
        hilt.transform.SetParent(sword.transform, false);
        hilt.transform.localScale = new Vector3(0.03f, 0.08f, 0.03f);
        hilt.transform.localPosition = Vector3.zero;
        Material hiltMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hiltMat.color = new Color(0.3f, 0.25f, 0.2f);
        hilt.GetComponent<Renderer>().sharedMaterial = hiltMat;

        return sword;
    }
}

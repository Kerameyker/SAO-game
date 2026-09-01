using UnityEngine;
using System.Collections;

/// <summary>
/// Повісь на Player. Клік лівою кнопкою миші б'є все з тегом "Enemy" в невеликому радіусі перед собою.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    public float attackRange = 2.5f;
    public float attackDamage = 15f;
    public float attackCooldown = 0.6f;

    private float cooldownTimer;
    private Transform visualModel;
    private Inventory inventory;

    void Start()
    {
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) visualModel = anim.transform;
        inventory = GetComponent<Inventory>();
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0f)
        {
            Attack();
            cooldownTimer = attackCooldown;
        }
    }

    void Attack()
    {
        if (visualModel != null) StartCoroutine(PunchFeedback());
        SpawnSlashArc();

        Vector3 center = transform.position + transform.forward * 1f + Vector3.up * 1f;
        Collider[] hits = Physics.OverlapSphere(center, attackRange);
        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            Health h = hit.GetComponent<Health>();
            if (h != null)
            {
                float totalDamage = attackDamage + (inventory != null ? inventory.TotalAttackBonus : 0f);
                h.TakeDamage(totalDamage);
                SpawnHitSpark(hit.bounds.center);
            }
        }
    }

    void SpawnSlashArc()
    {
        GameObject slash = new GameObject("SlashArc");
        slash.transform.position = transform.position + transform.forward * 1.3f + Vector3.up * 1.1f;
        slash.transform.rotation = transform.rotation * Quaternion.Euler(0f, 0f, 20f);

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(slash.transform, false);
        Destroy(visual.GetComponent<Collider>());
        visual.transform.localScale = new Vector3(1.1f, 0.08f, 0.03f);
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.95f, 0.95f, 0.8f);
        visual.GetComponent<Renderer>().sharedMaterial = mat;

        slash.AddComponent<HitSparkEffect>().lifetime = 0.15f;
        Destroy(slash, 0.2f);
    }

    IEnumerator PunchFeedback()
    {
        Vector3 original = visualModel.localScale;
        visualModel.localScale = original * 1.12f;
        yield return new WaitForSeconds(0.08f);
        visualModel.localScale = original;
    }

    void SpawnHitSpark(Vector3 pos)
    {
        GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(spark.GetComponent<Collider>());
        spark.transform.position = pos;
        spark.transform.localScale = Vector3.one * 0.3f;
        Renderer r = spark.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        r.material.color = Color.yellow;
        spark.AddComponent<HitSparkEffect>();
    }
}

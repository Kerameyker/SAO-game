using UnityEngine;

/// <summary>
/// Поведінка боса: чекає в зоні виявлення, переслідує й б'є сильніше за звичайних мобів,
/// при смерті гарантовано дає XP, золото і 2 предмети високої рідкості (не респавниться).
/// </summary>
[RequireComponent(typeof(Health))]
public class BossAI : MonoBehaviour
{
    private enum State { Idle, Chase, Attack, Dead }

    [Header("Виявлення і бій")]
    public float detectRange = 25f;
    public float attackRange = 3f;
    public float chaseSpeed = 4f;
    public float attackDamage = 20f;
    public float attackCooldown = 2f;

    [Header("Нагорода")]
    public int bossLevel = 10;
    public float xpReward = 500f;
    public int goldReward = 200;

    private Transform player;
    private Health health;
    private State state = State.Idle;
    private float attackTimer;

    void Start()
    {
        health = GetComponent<Health>();
        health.onDeath += Die;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (state == State.Dead || player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Idle:
                if (dist <= detectRange) state = State.Chase;
                break;
            case State.Chase:
                if (dist <= attackRange) { state = State.Attack; break; }
                MoveTowards(player.position);
                break;
            case State.Attack:
                if (dist > attackRange * 1.3f) { state = State.Chase; break; }
                FaceTarget(player.position);
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    attackTimer = attackCooldown;
                    Health playerHealth = player.GetComponent<Health>();
                    Inventory playerInv = player.GetComponent<Inventory>();
                    float dmg = attackDamage - (playerInv != null ? playerInv.TotalDefenseBonus : 0f);
                    dmg = Mathf.Max(2f, dmg);
                    if (playerHealth != null) playerHealth.TakeDamage(dmg);
                }
                break;
        }
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.magnitude < 0.3f) return;
        dir.Normalize();
        transform.position += dir * chaseSpeed * Time.deltaTime;
        FaceTarget(transform.position + dir);
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 4f * Time.deltaTime);
    }

    void Die()
    {
        state = State.Dead;
        if (player == null) return;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null) stats.AddXP(xpReward);

        Gold gold = player.GetComponent<Gold>();
        if (gold != null) gold.Add(goldReward);

        Inventory inv = player.GetComponent<Inventory>();
        if (inv != null)
        {
            for (int i = 0; i < 2; i++)
            {
                ItemData item = LootTable.RollBossItem(bossLevel);
                inv.AddItem(item);
                Debug.Log("Бос дав: " + item.itemName + " (" + ItemData.GetRarityLabel(item.rarity) + ")");
            }
        }

        Debug.Log("Боса переможено!");
    }
}

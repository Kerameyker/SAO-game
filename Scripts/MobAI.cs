using UnityEngine;
using System.Collections;

/// <summary>
/// Проста поведінка дикого моба: блукає навколо точки появи, помічає гравця
/// і переслідує, атакує в упор, гине коли HP=0. Рух прямою лінією (без NavMesh) -
/// достатньо для трави й відкритої місцевості, для складнішого оточення
/// (обхід перешкод) варто буде згодом підключити Unity NavMesh.
/// </summary>
[RequireComponent(typeof(Health))]
public class MobAI : MonoBehaviour
{
    private enum State { Wander, Chase, Attack, Dead }

    [Header("Виявлення гравця")]
    public float detectRange = 10f;
    public float attackRange = 2f;
    public float loseRange = 16f;

    [Header("Рух")]
    public float wanderSpeed = 1.5f;
    public float chaseSpeed = 3.2f;
    public float wanderRadius = 8f;
    public float wanderInterval = 4f;

    [Header("Бій")]
    public float attackDamage = 5f;
    public float attackCooldown = 1.5f;
    public int mobLevel = 1;
    public float xpReward = 15f;
    public int goldReward = 5;
    [Range(0f,1f)] public float lootChance = 0.35f;
    public float respawnDelay = 30f;

    private Transform player;
    private Health health;
    private Vector3 spawnPoint;
    private Vector3 wanderTarget;
    private float wanderTimer;
    private float attackTimer;
    private State state = State.Wander;

    void Start()
    {
        health = GetComponent<Health>();
        health.onDeath += Die;
        spawnPoint = transform.position;
        PickNewWanderTarget();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (state == State.Dead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Wander:
                Wander();
                if (dist <= detectRange) state = State.Chase;
                break;
            case State.Chase:
                if (dist > loseRange) { state = State.Wander; break; }
                if (dist <= attackRange) { state = State.Attack; break; }
                MoveTowards(player.position, chaseSpeed);
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
                    dmg = Mathf.Max(1f, dmg);
                    if (playerHealth != null) playerHealth.TakeDamage(dmg);
                }
                break;
        }
    }

    void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f) PickNewWanderTarget();
        MoveTowards(wanderTarget, wanderSpeed);
    }

    void PickNewWanderTarget()
    {
        Vector2 rnd = Random.insideUnitCircle * wanderRadius;
        wanderTarget = spawnPoint + new Vector3(rnd.x, 0f, rnd.y);
        wanderTimer = wanderInterval;
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.magnitude < 0.3f) return;
        dir.Normalize();
        transform.position += dir * speed * Time.deltaTime;
        FaceTarget(transform.position + dir);
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
    }

    void Die()
    {
        state = State.Dead;
        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                float levelDiff = stats.level - mobLevel;
                // якщо гравець сильно переріс цього моба - фарм дає дедалі менше XP
                float multiplier = levelDiff > 0f ? Mathf.Clamp01(1f - levelDiff * 0.15f) : 1f;
                stats.AddXP(xpReward * multiplier);
            }

            Gold gold = player.GetComponent<Gold>();
            if (gold != null)
            {
                gold.Add(goldReward + mobLevel * 2);
            }

            if (Random.value < lootChance)
            {
                Inventory inv = player.GetComponent<Inventory>();
                if (inv != null)
                {
                    ItemData item = LootTable.RollItem(mobLevel);
                    inv.AddItem(item);
                    Debug.Log("Випало: " + item.itemName + " (" + ItemData.GetRarityLabel(item.rarity) + ")");
                }
            }
        }
        SetVisible(false);
        StartCoroutine(RespawnRoutine());
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = visible;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = visible;
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        transform.position = spawnPoint;
        health.Revive();
        state = State.Wander;
        PickNewWanderTarget();
        SetVisible(true);
    }
}

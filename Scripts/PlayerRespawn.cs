using UnityEngine;
using System.Collections;

/// <summary>
/// Повісь на Player. Слухає Health.onDeath - при 0 HP тілепортує гравця
/// назад на точку спавну (запам'ятовану при старті) і повертає повне HP.
/// </summary>
[RequireComponent(typeof(Health))]
public class PlayerRespawn : MonoBehaviour
{
    public float respawnDelay = 2f;

    private Vector3 spawnPoint;
    private Health health;
    private CharacterController controller;
    private bool isDead = false;

    void Awake()
    {
        health = GetComponent<Health>();
        controller = GetComponent<CharacterController>();
        spawnPoint = transform.position;
    }

    void Start()
    {
        health.onDeath += HandleDeath;
    }

    void HandleDeath()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("Ви загинули. Відродження через " + respawnDelay + " сек.");
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        if (controller != null) controller.enabled = false;
        yield return new WaitForSeconds(respawnDelay);
        transform.position = spawnPoint;
        if (controller != null) controller.enabled = true;
        health.Revive();
        isDead = false;
    }
}

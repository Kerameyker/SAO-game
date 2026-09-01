using UnityEngine;
using System.Collections;

/// <summary>
/// Повісь на будь-який об'єкт з Health і Renderer (гравець, моб) -
/// коротко "спалахне" кольором при отриманні шкоди.
/// </summary>
[RequireComponent(typeof(Health))]
public class HitFlash : MonoBehaviour
{
    public Color flashColor = Color.white;
    public float flashDuration = 0.15f;

    private Renderer rend;
    private Color originalColor;
    private Health health;
    private float lastHealth;

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
        health = GetComponent<Health>();
        if (rend != null) originalColor = rend.material.color;
        lastHealth = health.currentHealth;
    }

    void Update()
    {
        if (health.currentHealth < lastHealth)
        {
            StopAllCoroutines();
            StartCoroutine(Flash());
        }
        lastHealth = health.currentHealth;
    }

    IEnumerator Flash()
    {
        if (rend == null) yield break;
        rend.material.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        rend.material.color = originalColor;
    }
}

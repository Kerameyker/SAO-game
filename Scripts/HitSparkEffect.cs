using UnityEngine;

/// <summary>
/// Короткий візуальний "спалах" удару - росте й зникає сам. Створюється кодом
/// у PlayerCombat.cs в момент влучання, не потребує додавання вручну.
/// </summary>
public class HitSparkEffect : MonoBehaviour
{
    public float lifetime = 0.2f;
    public float growTo = 1.6f;
    private float timer;
    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / lifetime);
        transform.localScale = startScale * Mathf.Lerp(1f, growTo, t);
    }
}

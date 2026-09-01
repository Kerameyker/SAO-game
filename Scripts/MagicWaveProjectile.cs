using UnityEngine;

/// <summary>
/// Летить уперед по прямій, завдає шкоди першому ворогу, якого торкнеться,
/// і зникає після максимальної дистанції.
/// </summary>
public class MagicWaveProjectile : MonoBehaviour
{
    public float speed = 14f;
    public float damage = 25f;
    public float maxDistance = 8f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;
        Health h = other.GetComponent<Health>();
        if (h != null) h.TakeDamage(damage);
        Destroy(gameObject);
    }
}

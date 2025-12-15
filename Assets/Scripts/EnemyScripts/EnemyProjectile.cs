using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 4f;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Called by RangedShooter when spawning
    public void Fire(Vector2 direction)
    {
        _rb.velocity = direction.normalized * speed;
        transform.right = direction;  // face travel direction
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit player?
        if (other.TryGetComponent<PlayerHealth>(out var player))
        {
            player.TakeDamage(damage, transform.position, false);  // no knockback
            Destroy(gameObject);
            return;
        }
    }
}

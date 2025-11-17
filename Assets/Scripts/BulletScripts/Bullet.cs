using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody2D _rb;
    private int _damage;
    private int _pierceRemaining;
    private float _ttl;

    private void Awake() { _rb = GetComponent<Rigidbody2D>(); }

    // Called by the shooter after Instantiate
    public void Init(Vector2 direction, float speed, int damage, int pierce)
    {
        _damage = damage;
        _pierceRemaining = pierce;
        _ttl = lifeTime;

        _rb.velocity = direction.normalized * speed;
        transform.right = direction; // rotate sprite to flight direction
    }

    private void Update()
    {
        _ttl -= Time.deltaTime;
        if (_ttl <= 0f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only damage enemies
        if (other.TryGetComponent<EnemyHealth>(out var hp))
        {
            hp.TakeDamage(_damage);

            if (_pierceRemaining > 0) _pierceRemaining--;
            else Destroy(gameObject);
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BulletElemental : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody2D _rb;
    private WeaponDefinition _weapon;
    private float _ttl;
    private int _pierceLeft;

    public void Init(WeaponDefinition weapon, Vector2 dir)
    {
        _weapon = weapon;
        _pierceLeft = weapon.pierce;
        _ttl = lifeTime;

        if (!_rb) _rb = GetComponent<Rigidbody2D>();
        _rb.velocity = dir.normalized * weapon.bulletSpeed;
        transform.right = dir; // rotate to direction
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _ttl -= Time.deltaTime;
        if (_ttl <= 0f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only damage enemies
        if (!other.TryGetComponent<EnemyHealth>(out var enemy)) return;

        // Build the damage packet
        var packet = new DamagePacket
        {
            amount = _weapon.baseDamage,
            element = _weapon.element,
            splashRadius = _weapon.splashRadius,
            sourcePos = transform.position
        };

        // Single target damage first
        enemy.TakeDamage(packet);

        // Optional splash damage
        if (_weapon.splashRadius > 0.01f)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, _weapon.splashRadius, ~0);
            foreach (var hit in hits)
            {
                if (hit == other) continue; // skip the primary we already hit
                if (hit.TryGetComponent<EnemyHealth>(out var e2))
                {
                    e2.TakeDamage(packet);
                }
            }
        }

        // Handle pierce
        if (_pierceLeft > 0) { _pierceLeft--; return; }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (_weapon && _weapon.splashRadius > 0.01f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _weapon.splashRadius);
        }
    }
}

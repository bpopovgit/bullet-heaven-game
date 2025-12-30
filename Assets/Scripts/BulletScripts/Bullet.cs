using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 3f;

    [Header("Damage")]
    [SerializeField] private DamageElement element = DamageElement.Physical;

    [Header("Collision")]
    [Tooltip("Set this to the Walls layer (only).")]
    [SerializeField] private LayerMask wallsMask;

    private Rigidbody2D _rb;
    private Collider2D _col;

    private int _damage;
    private int _pierceRemaining;
    private float _ttl;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

        // This script expects trigger callbacks.
        _col.isTrigger = true;

        // Lifetime countdown
        _ttl = lifeTime;
    }

    // Called by the shooter after Instantiate
    public void Init(Vector2 direction, float speed, int damage, int pierce)
    {
        _damage = damage;
        _pierceRemaining = pierce;

        direction = direction.normalized;
        _rb.velocity = direction * speed;

        // Optional: rotate sprite to face travel direction
        if (direction.sqrMagnitude > 0.0001f)
            transform.right = direction;
    }

    private void Update()
    {
        _ttl -= Time.deltaTime;
        if (_ttl <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1) Hit a wall? Destroy BULLET only.
        if (IsInLayerMask(other.gameObject.layer, wallsMask))
        {
            Destroy(gameObject);
            return;
        }

        // 2) Damage enemies
        if (other.TryGetComponent<EnemyHealth>(out var hp))
        {
            DamagePacket packet = new DamagePacket(_damage, element);
            hp.TakeDamage(packet);

            if (_pierceRemaining > 0)
            {
                _pierceRemaining--;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}

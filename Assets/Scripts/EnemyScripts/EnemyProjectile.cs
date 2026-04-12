using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;

    [Header("Damage")]
    [SerializeField] private int damage = 10;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 4f;

    [Header("Collision")]
    [Tooltip("Set this to the Walls layer (only). Enemy projectile disappears on wall hit.")]
    [SerializeField] private LayerMask wallsMask;
    [Header("Element & Status")]
    [SerializeField] private DamageElement element = DamageElement.Physical;
    [SerializeField] private StatusEffect status = StatusEffect.None;
    [SerializeField] private float statusDuration = 0f;
    [Range(0f, 1f)]
    [SerializeField] private float statusStrength = 0f;


    private Rigidbody2D _rb;
    private Collider2D _col;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

        // This script uses trigger callbacks.
        _col.isTrigger = true;
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
        Debug.Log($"EnemyProjectile hit: {other.name}");

        // Hit a wall? Destroy projectile only.
        if (IsInLayerMask(other.gameObject.layer, wallsMask))
        {
            Debug.Log("Projectile hit wall");
            Destroy(gameObject);
            return;
        }

        // Hit player?
        if (other.TryGetComponent<PlayerHealth>(out var player))
        {
            Debug.Log($"PROJECTILE HIT PLAYER. Element = {element}, Status = {status}");

            var packet = new DamagePacket
            {
                amount = damage,
                element = element,
                splashRadius = 0f,
                sourcePos = transform.position,

                status = status,
                statusDuration = statusDuration,
                statusStrength = statusStrength
            };

            packet.Clamp();
            player.TakeDamage(packet, applyKnockback: false);
            Destroy(gameObject);
            return;
        }
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}

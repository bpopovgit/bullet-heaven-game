using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BulletElemental : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 3f;

    [Header("Collision")]
    [Tooltip("Set this to the Walls layer (only). Bullet will disappear on wall hit.")]
    [SerializeField] private LayerMask wallsMask;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private WeaponDefinition _weapon;
    private int _damage;
    private float _splashRadius;
    private float _ttl;
    private int _pierceLeft;

    public void Init(WeaponDefinition weapon, Vector2 dir, PlayerStats ownerStats = null)
    {
        _weapon = weapon;
        float damageMultiplier = ownerStats != null ? ownerStats.DamageMultiplier : 1f;

        _damage = Mathf.Max(0, Mathf.RoundToInt(weapon.baseDamage * damageMultiplier));
        _splashRadius = weapon.splashRadius + (ownerStats != null ? ownerStats.SplashRadiusBonus : 0f);
        _pierceLeft = weapon.pierce + (ownerStats != null ? ownerStats.BonusPierce : 0);
        _ttl = lifeTime;

        if (!_rb) _rb = GetComponent<Rigidbody2D>();
        _rb.linearVelocity = dir.normalized * weapon.bulletSpeed;
        transform.right = dir; // rotate to direction
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

        // This script uses trigger callbacks.
        _col.isTrigger = true;
    }

    private void Update()
    {
        _ttl -= Time.deltaTime;
        if (_ttl <= 0f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1) Hit a wall? Destroy BULLET only.
        if (IsInLayerMask(other.gameObject.layer, wallsMask))
        {
            Destroy(gameObject);
            return;
        }

        // 2) Only damage enemies
        if (!other.TryGetComponent<EnemyHealth>(out var enemy)) return;

        // Build the damage packet
        var packet = new DamagePacket
        {
            amount = _damage,
            element = _weapon.element,
            splashRadius = _splashRadius,
            sourcePos = transform.position,
            status = ShouldApplyWeaponStatus() ? _weapon.onHitEffect : StatusEffect.None,
            statusDuration = _weapon.statusDuration,
            statusStrength = _weapon.statusStrength
        };

        // Single target damage first
        enemy.TakeDamage(packet);

        // Optional splash damage
        if (_splashRadius > 0.01f)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, _splashRadius, ~0);
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

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private bool ShouldApplyWeaponStatus()
    {
        if (_weapon == null || _weapon.onHitEffect == StatusEffect.None)
            return false;

        return Random.value <= _weapon.effectChance;
    }

    private void OnDrawGizmosSelected()
    {
        float radius = _splashRadius > 0.01f ? _splashRadius : (_weapon ? _weapon.splashRadius : 0f);

        if (radius > 0.01f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}

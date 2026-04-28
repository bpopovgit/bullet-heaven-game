using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FactionRangedAttacker : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private float preferredRange = 5.5f;
    [SerializeField] private float attackRange = 8.5f;
    [SerializeField] private float moveSpeed = 2.35f;
    [SerializeField] private float targetRefreshInterval = 0.25f;

    [Header("Projectile")]
    [SerializeField] private float fireCooldown = 1.05f;
    [SerializeField] private int projectileDamage = 6;
    [SerializeField] private float projectileSpeed = 11.5f;
    [SerializeField] private float projectileSpawnOffset = 0.5f;
    [SerializeField] private DamageElement projectileElement = DamageElement.Lightning;
    [SerializeField] private StatusEffect projectileStatus = StatusEffect.None;
    [SerializeField] private float statusDuration = 0f;
    [Range(0f, 1f)]
    [SerializeField] private float statusStrength = 0f;
    [SerializeField] private Color projectileColor = new Color(1f, 0.9f, 0.28f, 1f);

    private Rigidbody2D _rb;
    private StatusReceiver _status;
    private FactionMember _faction;
    private FactionMember _target;
    private float _nextTargetRefreshTime;
    private float _shotTimer;

    public void ConfigureCombat(
        float newPreferredRange,
        float newAttackRange,
        float newMoveSpeed,
        float newFireCooldown,
        int newProjectileDamage,
        float newProjectileSpeed,
        DamageElement newProjectileElement,
        StatusEffect newProjectileStatus,
        float newStatusDuration,
        float newStatusStrength,
        Color newProjectileColor)
    {
        preferredRange = Mathf.Max(0.5f, newPreferredRange);
        attackRange = Mathf.Max(preferredRange, newAttackRange);
        moveSpeed = Mathf.Max(0f, newMoveSpeed);
        fireCooldown = Mathf.Max(0.1f, newFireCooldown);
        projectileDamage = Mathf.Max(1, newProjectileDamage);
        projectileSpeed = Mathf.Max(0.1f, newProjectileSpeed);
        projectileElement = newProjectileElement;
        projectileStatus = newProjectileStatus;
        statusDuration = Mathf.Max(0f, newStatusDuration);
        statusStrength = Mathf.Clamp01(newStatusStrength);
        projectileColor = newProjectileColor;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        _status = GetComponent<StatusReceiver>();
        if (_status == null)
            _status = gameObject.AddComponent<StatusReceiver>();

        _faction = FactionMember.Ensure(gameObject, FactionType.Zombie);
    }

    private void Update()
    {
        if (_status != null && _status.IsStunned)
            return;

        if (_target == null || Time.time >= _nextTargetRefreshTime)
            RefreshTarget();

        _shotTimer -= Time.deltaTime;
        if (_target != null && _shotTimer <= 0f)
            ShootAtTarget();
    }

    private void FixedUpdate()
    {
        if (_rb == null)
            return;

        if (_status != null && _status.IsStunned)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        if (_target == null)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toTarget = _target.transform.position - transform.position;
        float distance = toTarget.magnitude;
        if (distance <= 0.001f)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 desiredVelocity = Vector2.zero;
        if (distance > preferredRange * 1.12f)
            desiredVelocity = toTarget.normalized;
        else if (distance < preferredRange * 0.82f)
            desiredVelocity = -toTarget.normalized;

        float speedMultiplier = _status != null ? _status.SpeedMultiplier : 1f;
        _rb.linearVelocity = desiredVelocity * moveSpeed * speedMultiplier;
        transform.right = toTarget.normalized;
    }

    private void RefreshTarget()
    {
        _nextTargetRefreshTime = Time.time + Mathf.Max(0.05f, targetRefreshInterval);

        if (_faction == null)
            _faction = FactionMember.Ensure(gameObject, FactionType.Zombie);

        _target = FactionTargeting.FindBestTarget(_faction, transform.position, attackRange);
    }

    private void ShootAtTarget()
    {
        Vector2 direction = _target.transform.position - transform.position;
        if (direction.sqrMagnitude <= 0.001f)
            return;

        SpawnProjectile(direction.normalized);
        _shotTimer = fireCooldown;
        GameAudio.PlayEnemyShoot();
    }

    private void SpawnProjectile(Vector2 direction)
    {
        GameObject projectile = new GameObject($"{_faction.Faction}Projectile");
        projectile.transform.position = transform.position + (Vector3)(direction * projectileSpawnOffset);
        projectile.transform.localScale = Vector3.one * 0.2f;

        PickupSpriteFactory.AddDefaultRenderer(projectile, projectileColor, sortingOrder: 4);

        CircleCollider2D projectileCollider = projectile.AddComponent<CircleCollider2D>();
        projectileCollider.isTrigger = true;
        projectileCollider.radius = 0.42f;

        Rigidbody2D projectileBody = projectile.AddComponent<Rigidbody2D>();
        projectileBody.gravityScale = 0f;
        projectileBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        FactionProjectile factionProjectile = projectile.AddComponent<FactionProjectile>();
        factionProjectile.Configure(
            direction,
            _faction,
            projectileDamage,
            projectileSpeed,
            projectileElement,
            projectileStatus,
            statusDuration,
            statusStrength);
    }
}

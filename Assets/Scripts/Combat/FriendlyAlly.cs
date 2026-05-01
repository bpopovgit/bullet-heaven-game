using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FriendlyAlly : MonoBehaviour
{
    [Header("Formation")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float arriveDistance = 0.12f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 7.5f;
    [SerializeField] private float targetRefreshInterval = 0.25f;
    [SerializeField] private float fireCooldown = 0.85f;
    [SerializeField] private int projectileDamage = 5;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileSpawnOffset = 0.45f;
    [SerializeField] private Color projectileColor = new Color(1f, 0.86f, 0.28f, 1f);

    private Transform _followTarget;
    private Vector2 _formationOffset;
    private Rigidbody2D _rb;
    private StatusReceiver _status;
    private FactionMember _faction;
    private FactionMember _target;
    private float _nextTargetRefreshTime;
    private float _shotTimer;

    public void Configure(Transform followTarget, Vector2 formationOffset)
    {
        _followTarget = followTarget;
        _formationOffset = formationOffset;
        IgnoreCollisionWithFollowTarget();
    }

    private void IgnoreCollisionWithFollowTarget()
    {
        if (_followTarget == null)
            return;

        Collider2D[] mine = GetComponentsInChildren<Collider2D>();
        Collider2D[] target = _followTarget.GetComponentsInChildren<Collider2D>();

        for (int i = 0; i < mine.Length; i++)
        {
            if (mine[i] == null) continue;
            for (int j = 0; j < target.Length; j++)
            {
                if (target[j] == null) continue;
                Physics2D.IgnoreCollision(mine[i], target[j], true);
            }
        }
    }

    public void ConfigureCombat(
        float moveSpeed,
        float attackRange,
        float fireCooldown,
        int projectileDamage,
        float projectileSpeed,
        Color projectileColor)
    {
        this.moveSpeed = Mathf.Max(0f, moveSpeed);
        this.attackRange = Mathf.Max(0.5f, attackRange);
        this.fireCooldown = Mathf.Max(0.1f, fireCooldown);
        this.projectileDamage = Mathf.Max(1, projectileDamage);
        this.projectileSpeed = Mathf.Max(0.1f, projectileSpeed);
        this.projectileColor = projectileColor;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        _status = GetComponent<StatusReceiver>();
        if (_status == null)
            _status = gameObject.AddComponent<StatusReceiver>();

        _faction = FactionMember.Ensure(gameObject, FactionType.Human);
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
        if (_followTarget == null)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 desiredPosition = (Vector2)_followTarget.position + _formationOffset;
        Vector2 currentPosition = _rb.position;
        Vector2 toDesired = desiredPosition - currentPosition;

        if (toDesired.magnitude <= arriveDistance)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float speedMultiplier = _status != null ? _status.SpeedMultiplier : 1f;
        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            desiredPosition,
            moveSpeed * speedMultiplier * Time.fixedDeltaTime);

        _rb.MovePosition(nextPosition);
    }

    private void RefreshTarget()
    {
        _nextTargetRefreshTime = Time.time + Mathf.Max(0.05f, targetRefreshInterval);
        _target = FactionTargeting.FindBestTarget(_faction, transform.position, attackRange);
    }

    private void ShootAtTarget()
    {
        if (_target == null)
            return;

        Vector2 direction = _target.transform.position - transform.position;
        if (direction.sqrMagnitude <= 0.001f)
            return;

        SpawnProjectile(direction.normalized);
        _shotTimer = fireCooldown;
    }

    private void SpawnProjectile(Vector2 direction)
    {
        GameObject projectile = new GameObject("HumanAllyProjectile");
        projectile.transform.position = transform.position + (Vector3)(direction * projectileSpawnOffset);
        projectile.transform.localScale = Vector3.one * 0.22f;

        PickupSpriteFactory.AddDefaultRenderer(projectile, projectileColor, sortingOrder: 4);

        CircleCollider2D projectileCollider = projectile.AddComponent<CircleCollider2D>();
        projectileCollider.isTrigger = true;
        projectileCollider.radius = 0.45f;

        Rigidbody2D projectileBody = projectile.AddComponent<Rigidbody2D>();
        projectileBody.gravityScale = 0f;
        projectileBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        FactionProjectile factionProjectile = projectile.AddComponent<FactionProjectile>();
        factionProjectile.Configure(
            direction,
            _faction,
            projectileDamage,
            projectileSpeed,
            DamageElement.Physical);
    }
}

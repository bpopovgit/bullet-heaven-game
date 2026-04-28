using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RangedShooter : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private float preferredRange = 6f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float targetRefreshInterval = 0.25f;
    [SerializeField] private float maxTargetRange = 0f;

    [Header("Shooting")]
    [SerializeField] private GameObject enemyProjectilePrefab;
    [SerializeField] private float fireCooldown = 1.2f;

    private Transform _target;
    private Rigidbody2D _rb;
    private StatusReceiver _status;
    private FactionMember _faction;
    private float _cd;
    private float _nextTargetRefreshTime;

    public GameObject EnemyProjectilePrefab => enemyProjectilePrefab;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _status = GetComponent<StatusReceiver>();
        _faction = FactionMember.Ensure(gameObject, FactionType.Zombie);
    }

    private void Start()
    {
        if (_status == null)
            _status = GetComponent<StatusReceiver>();

        RefreshTarget();
    }

    private void FixedUpdate()
    {
        if (_target == null || Time.time >= _nextTargetRefreshTime)
            RefreshTarget();

        if (!_target) return;

        Vector2 toTarget = (_target.position - transform.position);
        float dist = toTarget.magnitude;

        // Maintain preferred range
        Vector2 desired = Vector2.zero;
        if (dist > preferredRange * 1.1f)
            desired = toTarget.normalized;          // approach
        else if (dist < preferredRange * 0.9f)
            desired = -toTarget.normalized;         // back away

        float speedMultiplier = _status != null ? _status.SpeedMultiplier : 1f;
        _rb.linearVelocity = desired * moveSpeed * speedMultiplier;
        transform.right = toTarget.normalized;
    }

    private void Update()
    {
        if (_target == null || Time.time >= _nextTargetRefreshTime)
            RefreshTarget();

        if (!_target) return;

        if (_status != null && _status.IsStunned)
            return;

        _cd -= Time.deltaTime;
        if (_cd <= 0f)
        {
            _cd = fireCooldown;
            Fire();
        }
    }

    private void Fire()
    {
        if (!enemyProjectilePrefab) return;

        Vector2 dir = (_target.position - transform.position).normalized;

        var go = Instantiate(enemyProjectilePrefab, transform.position, Quaternion.identity);
        var proj = go.GetComponent<EnemyProjectile>();
        if (proj != null)
        {
            proj.Fire(dir, ownerFaction: _faction);
            GameAudio.PlayEnemyShoot();
        }
        else
        {
            Debug.LogError("EnemyProjectile prefab is missing the EnemyProjectile script!");
        }
    }

    private void RefreshTarget()
    {
        _nextTargetRefreshTime = Time.time + Mathf.Max(0.05f, targetRefreshInterval);

        if (_faction == null)
            _faction = FactionMember.Ensure(gameObject, FactionType.Zombie);

        FactionMember target = FactionTargeting.FindBestTarget(_faction, transform.position, maxTargetRange);
        _target = target != null ? target.transform : null;
    }
}

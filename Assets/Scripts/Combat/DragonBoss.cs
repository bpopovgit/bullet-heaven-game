using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class DragonBoss : MonoBehaviour
{
    [Header("Special Attack")]
    [SerializeField] private float specialVolleyCooldown = 3.5f;
    [SerializeField] private int volleyProjectileCount = 7;
    [SerializeField] private float volleySpreadAngle = 65f;
    [SerializeField] private float projectileSpeedMultiplier = 1.15f;
    [SerializeField] private float muzzleOffset = 0.8f;

    private EnemyHealth _health;
    private Transform _player;
    private GameObject _projectilePrefab;
    private float _cooldownRemaining;
    private bool _configured;

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            _player = playerObject.transform;
    }

    private void Update()
    {
        if (!_configured || _health == null || _health.IsDead || _player == null || _projectilePrefab == null)
            return;

        _cooldownRemaining -= Time.deltaTime;
        if (_cooldownRemaining > 0f)
            return;

        FireBreathVolley();
        _cooldownRemaining = Mathf.Max(0.5f, specialVolleyCooldown);
    }

    public void Configure(
        float healthMultiplier,
        float rewardMultiplier,
        float pickupDropChanceBonus,
        float scaleMultiplier,
        Color tintColor,
        GameObject projectilePrefab,
        float volleyCooldown,
        int projectilesPerVolley,
        float spreadAngle,
        float speedMultiplier)
    {
        if (_configured)
            return;

        _configured = true;
        _projectilePrefab = projectilePrefab;
        specialVolleyCooldown = Mathf.Max(0.5f, volleyCooldown);
        volleyProjectileCount = Mathf.Max(3, projectilesPerVolley);
        volleySpreadAngle = Mathf.Max(5f, spreadAngle);
        projectileSpeedMultiplier = Mathf.Max(0.1f, speedMultiplier);
        _cooldownRemaining = specialVolleyCooldown * 0.5f;

        if (_health != null)
            _health.ApplyEliteModifiers(healthMultiplier, rewardMultiplier, pickupDropChanceBonus);

        transform.localScale *= Mathf.Max(1f, scaleMultiplier);

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null)
                renderer.color = tintColor;
        }

        gameObject.name = $"Dragon Boss {gameObject.name}";
    }

    private void FireBreathVolley()
    {
        Vector2 baseDirection = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        if (baseDirection.sqrMagnitude <= 0.0001f)
            baseDirection = Vector2.right;

        transform.right = baseDirection;

        float startAngle = -volleySpreadAngle * 0.5f;
        float angleStep = volleyProjectileCount <= 1 ? 0f : volleySpreadAngle / (volleyProjectileCount - 1);
        Vector3 spawnPosition = transform.position + (Vector3)(baseDirection * muzzleOffset);

        for (int i = 0; i < volleyProjectileCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 shotDirection = Quaternion.Euler(0f, 0f, angle) * baseDirection;

            GameObject projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
            if (projectile.TryGetComponent<EnemyProjectile>(out EnemyProjectile enemyProjectile))
                enemyProjectile.Fire(shotDirection, projectileSpeedMultiplier);
        }

        GameAudio.PlayEnemyShoot();
    }
}

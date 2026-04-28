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

    [Header("Boss Bar")]
    [SerializeField] private float healthBarHeightPadding = 0.9f;
    [SerializeField] private Color phaseOneHealthBarColor = new Color(0.78f, 0.12f, 0.16f, 1f);

    [Header("Phase Two")]
    [SerializeField, Range(0.05f, 0.95f)] private float phaseTwoTriggerRatio = 0.5f;
    [SerializeField] private float phaseTwoVolleyCooldown = 2.35f;
    [SerializeField] private int phaseTwoProjectileCount = 9;
    [SerializeField] private float phaseTwoSpreadAngle = 95f;
    [SerializeField] private float phaseTwoProjectileSpeedMultiplier = 1.22f;
    [SerializeField] private Color phaseTwoTintColor = new Color(1f, 0.12f, 0.08f, 1f);
    [SerializeField] private Color phaseTwoHealthBarColor = new Color(1f, 0.86f, 0.22f, 1f);
    [SerializeField] private string phaseTwoAnnouncement = "DRAGON ENRAGED";

    private EnemyHealth _health;
    private Transform _player;
    private GameObject _projectilePrefab;
    private FactionMember _faction;
    private float _cooldownRemaining;
    private bool _configured;
    private bool _phaseTwoTriggered;
    private BossWorldHealthBar _healthBar;
    private SpriteRenderer[] _renderers;

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _faction = FactionMember.Ensure(gameObject, FactionType.Zombie);

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            _player = playerObject.transform;
    }

    private void Update()
    {
        if (!_configured || _health == null || _health.IsDead)
            return;

        TryEnterPhaseTwo();

        if (_player == null || _projectilePrefab == null)
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

        ApplyTint(tintColor);

        if (_health != null)
            _healthBar = BossWorldHealthBar.Create(transform, _health, new Vector3(0f, GetVisualTopOffset() + healthBarHeightPadding, 0f), phaseOneHealthBarColor);

        gameObject.name = $"Dragon Boss {gameObject.name}";
    }

    private void TryEnterPhaseTwo()
    {
        if (_phaseTwoTriggered || _health == null)
            return;

        float healthRatio = _health.CurrentHealth / (float)Mathf.Max(1, _health.MaxHealth);
        if (healthRatio > phaseTwoTriggerRatio)
            return;

        _phaseTwoTriggered = true;
        specialVolleyCooldown = Mathf.Max(0.5f, phaseTwoVolleyCooldown);
        volleyProjectileCount = Mathf.Max(3, phaseTwoProjectileCount);
        volleySpreadAngle = Mathf.Max(5f, phaseTwoSpreadAngle);
        projectileSpeedMultiplier = Mathf.Max(0.1f, phaseTwoProjectileSpeedMultiplier);
        _cooldownRemaining = 0.4f;

        ApplyTint(phaseTwoTintColor);

        if (_healthBar != null)
            _healthBar.SetFillColor(phaseTwoHealthBarColor);

        if (RunAnnouncementUI.Instance != null && !string.IsNullOrWhiteSpace(phaseTwoAnnouncement))
            RunAnnouncementUI.Instance.ShowMessage(phaseTwoAnnouncement, 2f);

        GameAudio.PlayEliteSpawn();
        Debug.Log("DRAGON PHASE TWO TRIGGERED");
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
                enemyProjectile.Fire(shotDirection, projectileSpeedMultiplier, _faction);
        }

        GameAudio.PlayEnemyShoot();
    }

    private float GetVisualTopOffset()
    {
        float bestY = 1.5f;

        if (_renderers == null || _renderers.Length == 0)
            return bestY;

        for (int i = 0; i < _renderers.Length; i++)
        {
            SpriteRenderer renderer = _renderers[i];
            if (renderer == null)
                continue;

            float localTop = renderer.bounds.max.y - transform.position.y;
            if (localTop > bestY)
                bestY = localTop;
        }

        return bestY;
    }

    private void ApplyTint(Color tintColor)
    {
        if (_renderers == null)
            return;

        foreach (SpriteRenderer renderer in _renderers)
        {
            if (renderer != null)
                renderer.color = tintColor;
        }
    }
}

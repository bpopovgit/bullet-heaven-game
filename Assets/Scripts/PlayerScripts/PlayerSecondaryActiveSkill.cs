using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSecondaryActiveSkill : MonoBehaviour
{
    private readonly Collider2D[] _enemyHits = new Collider2D[64];

    private SecondaryActiveSkillDefinition _config;
    private float _cooldownRemaining;
    private PlayerHealth _playerHealth;
    private Rigidbody2D _rigidbody2D;

    public bool IsConfigured => _config != null;
    public float CooldownRemaining => Mathf.Max(0f, _cooldownRemaining);
    public float CooldownDuration => _config != null ? Mathf.Max(0.01f, _config.cooldown) : 0f;
    public float CooldownNormalized => _config == null ? 0f : Mathf.Clamp01(_cooldownRemaining / Mathf.Max(0.01f, _config.cooldown));
    public string SkillDisplayName => _config != null ? _config.displayName : "Skill";
    public StartingSkillChoice SkillChoice { get; private set; } = StartingSkillChoice.MagneticPulse;
    public Color SkillPrimaryColor => _config != null ? _config.iconPrimaryColor : Color.white;
    public Color SkillSecondaryColor => _config != null ? _config.iconSecondaryColor : Color.white;

    private Camera _mainCamera;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_cooldownRemaining > 0f)
            _cooldownRemaining -= Time.deltaTime;

        if (Time.timeScale <= 0f)
            return;

        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
            return;

        TryActivate();
    }

    public void Configure(StartingSkillChoice choice)
    {
        SkillChoice = choice;
        _config = CreateConfig(choice);
        _cooldownRemaining = 0f;
        Debug.Log($"ACTIVE SKILL READY: {_config.displayName} on E ({_config.cooldown:0.#}s cooldown)");
    }

    public void ReduceCooldown(float seconds)
    {
        if (_config == null)
            return;

        _config.cooldown = Mathf.Max(3f, _config.cooldown - Mathf.Max(0f, seconds));
    }

    public void AddRadius(float amount)
    {
        if (_config == null)
            return;

        _config.radius += Mathf.Max(0f, amount);
    }

    public void AddDuration(float amount)
    {
        if (_config == null)
            return;

        _config.duration += Mathf.Max(0f, amount);
        _config.statusDuration += Mathf.Max(0f, amount);
    }

    private void TryActivate()
    {
        if (_config == null)
            return;

        if (_cooldownRemaining > 0f)
        {
            Debug.Log($"SKILL COOLDOWN: {_config.displayName} ready in {_cooldownRemaining:0.0}s");
            return;
        }

        TryPhaseShiftBlink();
        ActivateCurrentSkill();
        TryConsumeStatusForBurst();
        _cooldownRemaining = _config.cooldown;
        Debug.Log($"ACTIVE SKILL USED: {_config.displayName}");
    }

    private void TryPhaseShiftBlink()
    {
        PlayerCombatModifiers modifiers = PlayerCombatModifiers.Instance;
        if (modifiers == null || modifiers.SkillBlinkDistance <= 0f)
            return;

        if (_mainCamera == null)
            _mainCamera = Camera.main;
        if (_mainCamera == null || Mouse.current == null)
            return;

        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = transform.position.z;

        Vector2 toCursor = (Vector2)(mouseWorld - transform.position);
        float distance = Mathf.Min(modifiers.SkillBlinkDistance, toCursor.magnitude);
        if (distance <= 0.01f)
            return;

        Vector2 dir = toCursor.normalized;
        Vector2 newPosition = (Vector2)transform.position + dir * distance;

        if (_rigidbody2D != null)
            _rigidbody2D.position = newPosition;
        else
            transform.position = newPosition;

        Debug.Log($"PHASE SHIFT: blinked {distance:0.##}m toward cursor.");
    }

    private void TryConsumeStatusForBurst()
    {
        PlayerCombatModifiers modifiers = PlayerCombatModifiers.Instance;
        if (modifiers == null || modifiers.SkillElementBurstDamage <= 0f)
            return;

        Vector2 origin = transform.position;
        int count = Physics2D.OverlapCircleNonAlloc(origin, _config.radius, _enemyHits);
        int consumed = 0;

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = _enemyHits[i];
            if (hit == null || !hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
                continue;

            StatusReceiver receiver = hit.GetComponent<StatusReceiver>();
            if (receiver == null || !receiver.HasActiveStatus)
                continue;

            int burstDamage = Mathf.Max(1, Mathf.RoundToInt(modifiers.SkillElementBurstDamage));
            DamagePacket burstPacket = new DamagePacket(burstDamage, DamageElement.Physical, 0f, origin);
            enemy.TakeDamage(burstPacket);
            receiver.ClearMostRecentStatus();
            consumed++;
        }

        if (consumed > 0)
            Debug.Log($"ELEMENT UNLEASHED: consumed status on {consumed} enemies for +{Mathf.RoundToInt(modifiers.SkillElementBurstDamage)} damage each.");
    }

    private void ActivateCurrentSkill()
    {
        switch (_config.type)
        {
            case SecondaryActiveSkillType.ArcaneShield:
                ActivateArcaneShield();
                break;

            case SecondaryActiveSkillType.FrostNova:
                ActivateFrostNova();
                break;

            default:
                ActivateMagneticPulse();
                break;
        }
    }

    private void ActivateMagneticPulse()
    {
        Vector2 origin = transform.position;

        MagneticVortexEffect.Spawn(
            origin,
            _config.radius,
            Mathf.Max(1, _config.damage),
            Mathf.Max(_config.force, 11f),
            _config.iconPrimaryColor,
            _config.iconSecondaryColor);

        PlayerPickup[] pickups = FindObjectsOfType<PlayerPickup>();
        int attracted = 0;
        float pickupRadius = _config.radius * 2.4f;
        for (int i = 0; i < pickups.Length; i++)
        {
            PlayerPickup pickup = pickups[i];
            if (pickup == null)
                continue;

            if (Vector2.Distance(origin, pickup.transform.position) > pickupRadius)
                continue;

            pickup.AttractTo(transform);
            attracted++;
        }

        GameAudio.PlaySkillMagneticPulse();
        Debug.Log($"MAGNETIC VORTEX: spawned at {origin}, attracted {attracted} pickups, will pull-then-detonate.");
    }

    private void ActivateArcaneShield()
    {
        if (_playerHealth != null)
            _playerHealth.GrantTemporaryInvulnerability(_config.duration);

        EnemyProjectile[] projectiles = FindObjectsOfType<EnemyProjectile>();
        int cleared = 0;

        for (int i = 0; i < projectiles.Length; i++)
        {
            EnemyProjectile projectile = projectiles[i];
            if (projectile == null)
                continue;

            if (Vector2.Distance(transform.position, projectile.transform.position) > _config.radius)
                continue;

            Destroy(projectile.gameObject);
            cleared++;
        }

        GameAudio.PlaySkillArcaneShield();
        SecondarySkillVisual.SpawnAura(transform, _config.iconPrimaryColor, _config.iconSecondaryColor, _config.radius, _config.duration);
        Debug.Log($"ARCANE SHIELD: blocked incoming damage and cleared {cleared} nearby projectiles.");
    }

    private void ActivateFrostNova()
    {
        const float ShatterPrimeDuration = 4f;
        const float ShatterMultiplier = 1.85f;

        Vector2 origin = transform.position;
        int count = Physics2D.OverlapCircleNonAlloc(origin, _config.radius, _enemyHits);
        int frozen = 0;
        int primed = 0;

        DamagePacket packet = new DamagePacket(
            _config.damage,
            DamageElement.Frost,
            StatusEffect.Freeze,
            _config.statusDuration,
            _config.statusStrength,
            0f,
            origin);

        packet.Clamp();

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = _enemyHits[i];
            if (hit == null || !hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
                continue;

            enemy.TakeDamage(packet);
            frozen++;

            if (!enemy.IsDead)
            {
                enemy.PrimeForShatter(ShatterPrimeDuration, ShatterMultiplier);
                primed++;
            }
        }

        GameAudio.PlaySkillFrostNova();
        SecondarySkillVisual.SpawnPulse(transform.position, _config.iconPrimaryColor, _config.iconSecondaryColor, _config.radius, 0.55f);
        Debug.Log($"CRYO SHATTER: froze {frozen} enemies, primed {primed} for shatter (×{ShatterMultiplier}).");
    }

    private static SecondaryActiveSkillDefinition CreateConfig(StartingSkillChoice choice)
    {
        switch (choice)
        {
            case StartingSkillChoice.ArcaneShield:
                return new SecondaryActiveSkillDefinition
                {
                    displayName = "Arcane Shield",
                    type = SecondaryActiveSkillType.ArcaneShield,
                    cooldown = 16f,
                    radius = 3.2f,
                    duration = 2.4f,
                    force = 0f,
                    damage = 0,
                    statusDuration = 0f,
                    statusStrength = 0f,
                    iconPrimaryColor = new Color(0.45f, 0.88f, 1f, 1f),
                    iconSecondaryColor = new Color(0.76f, 0.95f, 1f, 0.92f)
                };

            case StartingSkillChoice.FrostNova:
                return new SecondaryActiveSkillDefinition
                {
                    displayName = "Cryo Shatter",
                    type = SecondaryActiveSkillType.FrostNova,
                    cooldown = 13f,
                    radius = 4.8f,
                    duration = 0.55f,
                    force = 0f,
                    damage = 0,
                    statusDuration = 2.2f,
                    statusStrength = 1f,
                    iconPrimaryColor = new Color(0.45f, 0.82f, 1f, 1f),
                    iconSecondaryColor = new Color(0.82f, 0.96f, 1f, 0.9f)
                };

            default:
                return new SecondaryActiveSkillDefinition
                {
                    displayName = "Magnetic Vortex",
                    type = SecondaryActiveSkillType.MagneticPulse,
                    cooldown = 12f,
                    radius = 5.2f,
                    duration = 0.95f,
                    force = 14f,
                    damage = 38,
                    statusDuration = 1.6f,
                    statusStrength = 0.35f,
                    iconPrimaryColor = new Color(0.42f, 1f, 0.72f, 1f),
                    iconSecondaryColor = new Color(0.95f, 1f, 0.78f, 0.9f)
                };
        }
    }
}

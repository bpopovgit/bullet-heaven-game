using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActiveBomb : MonoBehaviour
{
    private BombAbilityDefinition _config;
    private float _cooldownRemaining;
    private Camera _mainCamera;

    public bool IsConfigured => _config != null;
    public float CooldownRemaining => Mathf.Max(0f, _cooldownRemaining);
    public float CooldownDuration => _config != null ? Mathf.Max(0.01f, _config.cooldown) : 0f;
    public float CooldownNormalized => _config == null ? 0f : Mathf.Clamp01(_cooldownRemaining / Mathf.Max(0.01f, _config.cooldown));
    public string BombDisplayName => _config != null ? _config.displayName : "Bomb";
    public Color BombIconColor => _config != null ? _config.projectileColor : Color.white;
    public Color BombAccentColor => _config != null ? _config.explosionPrimaryColor : Color.white;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_cooldownRemaining > 0f)
            _cooldownRemaining -= Time.deltaTime;

        if (Time.timeScale <= 0f)
            return;

        if (Keyboard.current == null || !Keyboard.current.qKey.wasPressedThisFrame)
            return;

        TryActivate();
    }

    public void Configure(StartingBombChoice choice)
    {
        _config = CreateConfig(choice);
        _cooldownRemaining = 0f;
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        Debug.Log($"ACTIVE BOMB READY: {_config.displayName} on Q ({_config.cooldown:0.#}s cooldown, cursor-targeted)");
    }

    private void TryActivate()
    {
        if (_config == null)
            return;

        if (_cooldownRemaining > 0f)
        {
            Debug.Log($"BOMB COOLDOWN: {_config.displayName} ready in {_cooldownRemaining:0.0}s");
            return;
        }

        _cooldownRemaining = _config.cooldown;
        Vector3 targetPosition = GetTargetPosition();
        PlayerBombProjectile.Spawn(transform.position, targetPosition, _config);
        GameAudio.PlayBombThrow();
        Debug.Log($"ACTIVE BOMB THROWN: {_config.displayName} toward {targetPosition}");
    }

    private Vector3 GetTargetPosition()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        Vector3 origin = transform.position;
        Vector3 targetPosition = origin + Vector3.right * 2f;

        if (_mainCamera != null && Mouse.current != null)
        {
            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorld.z = 0f;

            Vector3 delta = mouseWorld - origin;
            if (delta.magnitude > _config.maxRange)
                delta = delta.normalized * _config.maxRange;

            targetPosition = origin + delta;
        }

        targetPosition.z = 0f;
        return targetPosition;
    }

    private static BombAbilityDefinition CreateConfig(StartingBombChoice choice)
    {
        switch (choice)
        {
            case StartingBombChoice.FrostBomb:
                return new BombAbilityDefinition
                {
                    displayName = "Frost Bomb",
                    element = DamageElement.Frost,
                    status = StatusEffect.Slow,
                    damage = 35,
                    radius = 5.4f,
                    cooldown = 12f,
                    statusDuration = 2.5f,
                    statusStrength = 0.45f,
                    projectileScale = 0.65f,
                    projectileSpeed = 16f,
                    maxRange = 10f,
                    projectileColor = new Color(0.55f, 0.88f, 1f, 1f),
                    explosionPrimaryColor = new Color(0.72f, 0.94f, 1f, 0.92f),
                    explosionSecondaryColor = new Color(0.35f, 0.65f, 1f, 0.8f)
                };

            case StartingBombChoice.FireBomb:
                return new BombAbilityDefinition
                {
                    displayName = "Fire Bomb",
                    element = DamageElement.Fire,
                    status = StatusEffect.Burn,
                    damage = 48,
                    radius = 4.8f,
                    cooldown = 13f,
                    statusDuration = 3f,
                    statusStrength = 0.34f,
                    projectileScale = 0.62f,
                    projectileSpeed = 18f,
                    maxRange = 10f,
                    projectileColor = new Color(1f, 0.46f, 0.14f, 1f),
                    explosionPrimaryColor = new Color(1f, 0.7f, 0.18f, 0.95f),
                    explosionSecondaryColor = new Color(1f, 0.2f, 0.08f, 0.82f)
                };

            case StartingBombChoice.ShockBomb:
                return new BombAbilityDefinition
                {
                    displayName = "Shock Bomb",
                    element = DamageElement.Lightning,
                    status = StatusEffect.Shock,
                    damage = 40,
                    radius = 4.6f,
                    cooldown = 10f,
                    statusDuration = 2.25f,
                    statusStrength = 0.4f,
                    projectileScale = 0.58f,
                    projectileSpeed = 19f,
                    maxRange = 10f,
                    projectileColor = new Color(1f, 0.96f, 0.3f, 1f),
                    explosionPrimaryColor = new Color(1f, 0.98f, 0.55f, 0.96f),
                    explosionSecondaryColor = new Color(0.52f, 0.8f, 1f, 0.84f)
                };

            default:
                return new BombAbilityDefinition
                {
                    displayName = "Frag Bomb",
                    element = DamageElement.Physical,
                    status = StatusEffect.None,
                    damage = 60,
                    radius = 4.9f,
                    cooldown = 12f,
                    statusDuration = 0f,
                    statusStrength = 0f,
                    projectileScale = 0.6f,
                    projectileSpeed = 17f,
                    maxRange = 10f,
                    projectileColor = new Color(1f, 0.85f, 0.24f, 1f),
                    explosionPrimaryColor = new Color(1f, 0.88f, 0.42f, 0.95f),
                    explosionSecondaryColor = new Color(1f, 0.52f, 0.16f, 0.82f)
                };
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSecondaryWeapon : MonoBehaviour
{
    private const float DefaultCooldown = 0.6f;

    private Camera _mainCam;
    private PlayerStats _stats;
    private FactionMember _faction;
    private PlayerShooting _shooting;
    private WeaponDefinition _runtimeWeapon;
    private float _cooldownRemaining;
    private string _displayName = "Secondary";

    public string DisplayName => _displayName;
    public bool IsConfigured => _runtimeWeapon != null;
    public float CooldownRemaining => Mathf.Max(0f, _cooldownRemaining);
    public float CooldownDuration => _runtimeWeapon != null ? Mathf.Max(0.05f, 1f / Mathf.Max(0.01f, _runtimeWeapon.shotsPerSecond)) : DefaultCooldown;
    public float CooldownNormalized => Mathf.Clamp01(_cooldownRemaining / CooldownDuration);

    public void ConfigureForCharacter(PlayableCharacterChoice character)
    {
        _shooting = GetComponent<PlayerShooting>();
        _stats = GetComponent<PlayerStats>();
        _faction = FactionMember.Ensure(gameObject, FactionType.Human);
        if (_mainCam == null)
            _mainCam = Camera.main;

        WeaponDefinition baseWeapon = _shooting != null ? _shooting.GetWeaponDefinition() : null;
        if (baseWeapon == null)
        {
            Debug.LogWarning("PlayerSecondaryWeapon: no base WeaponDefinition available; secondary weapon disabled.");
            _runtimeWeapon = null;
            return;
        }

        _runtimeWeapon = Object.Instantiate(baseWeapon);
        _runtimeWeapon.name = $"{baseWeapon.name}_SecondaryRuntime";

        switch (character)
        {
            case PlayableCharacterChoice.HumanRanger:
                _displayName = "Shock Pellet";
                _runtimeWeapon.element = DamageElement.Lightning;
                _runtimeWeapon.onHitEffect = StatusEffect.Shock;
                _runtimeWeapon.effectChance = 0.4f;
                _runtimeWeapon.statusDuration = 1.4f;
                _runtimeWeapon.statusStrength = 0.3f;
                _runtimeWeapon.baseDamage = 18;
                _runtimeWeapon.shotsPerSecond = 1.6f;
                _runtimeWeapon.bulletSpeed = 28f;
                _runtimeWeapon.splashRadius = 0f;
                _runtimeWeapon.pierce = 1;
                break;

            case PlayableCharacterChoice.HumanArcanist:
                _displayName = "Cinder Bolt";
                _runtimeWeapon.element = DamageElement.Fire;
                _runtimeWeapon.onHitEffect = StatusEffect.Burn;
                _runtimeWeapon.effectChance = 0.45f;
                _runtimeWeapon.statusDuration = 2.4f;
                _runtimeWeapon.statusStrength = 0.32f;
                _runtimeWeapon.baseDamage = 22;
                _runtimeWeapon.shotsPerSecond = 1.4f;
                _runtimeWeapon.bulletSpeed = 22f;
                _runtimeWeapon.splashRadius = 0.45f;
                _runtimeWeapon.pierce = 0;
                break;

            case PlayableCharacterChoice.HumanVanguard:
                // Vanguard's RMB is handled by PlayerSecondaryMelee (Shield Bash).
                _runtimeWeapon = null;
                _displayName = "Shield Bash";
                return;
            default:
                _displayName = "Frag Toss";
                _runtimeWeapon.element = DamageElement.Physical;
                _runtimeWeapon.onHitEffect = StatusEffect.None;
                _runtimeWeapon.effectChance = 0f;
                _runtimeWeapon.statusDuration = 0f;
                _runtimeWeapon.statusStrength = 0f;
                _runtimeWeapon.baseDamage = 26;
                _runtimeWeapon.shotsPerSecond = 1.5f;
                _runtimeWeapon.bulletSpeed = 18f;
                _runtimeWeapon.splashRadius = 0.7f;
                _runtimeWeapon.pierce = 0;
                break;
        }

        _cooldownRemaining = 0f;
        Debug.Log($"SECONDARY WEAPON READY: {_displayName} on RMB ({CooldownDuration:0.##}s cooldown)");
    }

    private void Update()
    {
        if (_cooldownRemaining > 0f)
            _cooldownRemaining -= Time.deltaTime;

        if (_runtimeWeapon == null || Time.timeScale <= 0f)
            return;

        if (Mouse.current == null || !Mouse.current.rightButton.isPressed)
            return;

        if (_cooldownRemaining > 0f)
            return;

        Fire();
        _cooldownRemaining = CooldownDuration;
    }

    private void Fire()
    {
        if (_runtimeWeapon == null || _runtimeWeapon.bulletPrefab == null)
            return;

        if (_mainCam == null)
            _mainCam = Camera.main;
        if (_mainCam == null || Mouse.current == null)
            return;

        Vector3 mouseWorld = _mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;
        Vector2 dir = ((Vector2)(mouseWorld - transform.position));
        if (dir.sqrMagnitude <= 0.0001f)
            dir = Vector2.right;
        dir = dir.normalized;

        Vector3 spawn = transform.position + (Vector3)(dir * 0.35f);
        GameObject go = Instantiate(_runtimeWeapon.bulletPrefab, spawn, Quaternion.identity);
        BulletElemental bullet = go.GetComponent<BulletElemental>();
        if (bullet != null)
            bullet.Init(_runtimeWeapon, dir, _stats, _faction);

        GameAudio.PlayPlayerShoot();
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerShooting : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform muzzle;           // FirePoint (child of Player)
    [SerializeField] private Camera mainCam;             // leave empty to auto-fill
    [SerializeField] private WeaponDefinition weapon;    // assign a WeaponDefinition asset
    [SerializeField] private float multiProjectileSpreadAngle = 18f;
    [SerializeField] private float multiProjectileSpawnOffset = 0.18f;

    private PlayerInput _playerInput;
    private InputAction _fire;
    private PlayerStats _stats;
    private float _cooldown;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _stats = GetComponent<PlayerStats>();
        if (!mainCam) mainCam = Camera.main;
    }

    private void OnEnable()
    {
        _fire = _playerInput.actions["Fire"]; // must match your action name
        _playerInput.actions.Enable();
    }

    private void Update()
    {
        _cooldown -= Time.deltaTime;
        if (_fire != null && _fire.IsPressed() && _cooldown <= 0f)
        {
            ShootOnce();
            float fireRateMultiplier = _stats != null ? _stats.FireRateMultiplier : 1f;
            _cooldown = 1f / Mathf.Max(0.01f, weapon.shotsPerSecond * fireRateMultiplier);
        }
    }

    private void ShootOnce()
    {
        if (!muzzle || !weapon || !weapon.bulletPrefab || !mainCam) return;

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - muzzle.position).normalized;

        int projectileCount = Mathf.Max(1, 1 + (_stats != null ? _stats.BonusProjectiles : 0));
        float totalSpread = multiProjectileSpreadAngle * Mathf.Max(0, projectileCount - 1);
        float startAngle = -totalSpread * 0.5f;
        Vector2 perpendicular = new Vector2(-dir.y, dir.x);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + multiProjectileSpreadAngle * i;
            Vector2 shotDir = Quaternion.Euler(0f, 0f, angle) * dir;
            float offsetIndex = i - (projectileCount - 1) * 0.5f;
            Vector3 spawnPosition = muzzle.position + (Vector3)(perpendicular * (offsetIndex * multiProjectileSpawnOffset));

            var go = Instantiate(weapon.bulletPrefab, spawnPosition, Quaternion.identity);
            var bullet = go.GetComponent<BulletElemental>();
            if (bullet != null)
                bullet.Init(weapon, shotDir, _stats);
        }

        GameAudio.PlayPlayerShoot();

        // rotate the muzzle/arm for visuals (optional)
        muzzle.right = dir;
    }
}

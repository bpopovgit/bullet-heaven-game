using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerShooting : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform muzzle;           // FirePoint (child of Player)
    [SerializeField] private Camera mainCam;             // leave empty to auto-fill
    [SerializeField] private WeaponDefinition weapon;    // assign a WeaponDefinition asset

    private PlayerInput _playerInput;
    private InputAction _fire;
    private float _cooldown;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
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
            _cooldown = 1f / Mathf.Max(0.01f, weapon.shotsPerSecond);
        }
    }

    private void ShootOnce()
    {
        if (!muzzle || !weapon || !weapon.bulletPrefab || !mainCam) return;

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - muzzle.position).normalized;

        var go = Instantiate(weapon.bulletPrefab, muzzle.position, Quaternion.identity);
        var bullet = go.GetComponent<BulletElemental>();
        bullet.Init(weapon, dir);

        // rotate the muzzle/arm for visuals (optional)
        muzzle.right = dir;
    }
}

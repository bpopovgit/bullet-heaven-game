using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform muzzle;          // FirePoint (child of Player)
    [SerializeField] private GameObject bulletPrefab;   // Bullet prefab
    [SerializeField] private Camera mainCam;            // leave empty to auto-grab

    [Header("Weapon Stats")]
    [SerializeField] private float shotsPerSecond = 6f; // fire rate
    [SerializeField] private int damage = 10;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private int pierce = 0;

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
        _fire = _playerInput.actions["Fire"]; // uses your existing action
        _playerInput.actions.Enable();
    }

    private void Update()
    {
        _cooldown -= Time.deltaTime;
        if (_fire != null && _fire.IsPressed() && _cooldown <= 0f)
        {
            ShootOnce();
            _cooldown = 1f / Mathf.Max(0.01f, shotsPerSecond);
        }
    }

    private void ShootOnce()
    {
        if (!muzzle || !bulletPrefab || !mainCam) return;

        // Aim at mouse
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - muzzle.position).normalized;

        var go = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
        var bullet = go.GetComponent<Bullet>();
        bullet.Init(dir, bulletSpeed, damage, pierce);

        // Optional: visually point the muzzle at the cursor
        muzzle.right = dir;
    }
}

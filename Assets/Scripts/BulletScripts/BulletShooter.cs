using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BulletShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;     // where bullets spawn
    [SerializeField] private GameObject bulletPrefab; // bullet prefab or pooled object

    private Camera mainCam;
    private InputAction fireAction;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        var input = GetComponent<PlayerInput>();
        fireAction = input.actions["Fire"]; // must exist in your Input Actions asset
        input.actions.Enable();
    }

    private void OnDisable()
    {
        fireAction = null;
    }

    private void Update()
    {
        // Single-click fire
        if (fireAction != null && fireAction.WasPressedThisFrame())
            Fire();
    }

    private void Fire()
    {
        if (firePoint == null || bulletPrefab == null)
        {
            Debug.LogWarning("FirePoint or BulletPrefab not assigned in BulletShooter.");
            return;
        }

        // Read mouse position (works with new Input System)
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        // Direction from firePoint toward cursor
        Vector2 direction = (mouseWorldPos - (Vector2)firePoint.position).normalized;

        // Spawn bullet and pass direction to its controller
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        var bulletMover = bulletObj.GetComponent<BulletMover>();
        if (bulletMover != null)
            bulletMover.SetDirection(direction);
    }
}
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RangedShooter : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private float preferredRange = 6f;
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject enemyProjectilePrefab;
    [SerializeField] private float fireCooldown = 1.2f;

    private Transform _player;
    private Rigidbody2D _rb;
    private float _cd;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) _player = playerObj.transform;
    }

    private void FixedUpdate()
    {
        if (!_player) return;

        Vector2 toPlayer = (_player.position - transform.position);
        float dist = toPlayer.magnitude;

        // Maintain preferred range
        Vector2 desired = Vector2.zero;
        if (dist > preferredRange * 1.1f)
            desired = toPlayer.normalized;          // approach
        else if (dist < preferredRange * 0.9f)
            desired = -toPlayer.normalized;         // back away

        _rb.velocity = desired * moveSpeed;
        transform.right = toPlayer.normalized;
    }

    private void Update()
    {
        if (!_player) return;

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

        Vector2 dir = (_player.position - transform.position).normalized;

        var go = Instantiate(enemyProjectilePrefab, transform.position, Quaternion.identity);
        var proj = go.GetComponent<EnemyProjectile>();
        if (proj != null)
        {
            proj.Fire(dir);
        }
        else
        {
            Debug.LogError("EnemyProjectile prefab is missing the EnemyProjectile script!");
        }
    }
}

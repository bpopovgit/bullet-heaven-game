using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyRanged : MonoBehaviour
{
    public float preferredRange = 6f;
    public float speed = 2.5f;
    public GameObject projectilePrefab;
    public float fireCooldown = 1.2f;
    public float projectileSpeed = 10f;

    Rigidbody2D _rb;
    Transform _player;
    float _cd;

    void Awake() { _rb = GetComponent<Rigidbody2D>(); }
    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) _player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (!_player) return;
        Vector2 toPlayer = _player.position - transform.position;
        float dist = toPlayer.magnitude;

        Vector2 desired = Vector2.zero;
        if (dist > preferredRange * 1.1f) desired = toPlayer.normalized;
        else if (dist < preferredRange * 0.9f) desired = -toPlayer.normalized;

        _rb.velocity = desired * speed;
        transform.right = toPlayer.normalized;
    }

    void Update()
    {
        if (!_player) return;
        _cd -= Time.deltaTime;
        if (_cd <= 0f)
        {
            _cd = fireCooldown;
            var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            var rb = go.GetComponent<Rigidbody2D>();
            Vector2 dir = (_player.position - transform.position).normalized;
            rb.velocity = dir * projectileSpeed;
            go.transform.right = dir;
        }
    }
}
